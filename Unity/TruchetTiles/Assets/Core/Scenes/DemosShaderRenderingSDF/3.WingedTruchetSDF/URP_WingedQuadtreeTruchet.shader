Shader "Custom/URP_WingedQuadtreeTruchet"
{
    Properties
    {
        [NoScaleOffset] _ShapeMapArray ("Skeleton Array (RGFloat)", 2DArray) = "white" {}
        _GridScale ("Base Grid Scale", Float) = 4.0
        
        // Carlson Constant: 1/6 (0.166666)
        _Thickness ("Line Thickness (Constant)", Range(0.01, 0.35)) = 0.166666
        
        _SubdivisionChance ("Subdivision Probability", Range(0.0, 1.0)) = 0.6
        
        [Header(MultiScale Colors)]
        _ColorBg ("Background Color (Negative Space)", Color) = (0.8, 0.8, 0.8, 1.0)
        _ColorTile1 ("Primary Color", Color) = (0.2, 0.5, 0.9, 1.0) 
        _ColorTile2 ("Secondary Color", Color) = (0.05, 0.05, 0.05, 1.0) 
        
        [Header(Debug)]
        [Toggle] _ShowGrid ("Show Quadtree Grid", Float) = 1.0
        _GridColor ("Grid Color", Color) = (1.0, 0.0, 0.0, 1.0)
        _BoundsThickness ("Grid Thickness", Range(0.001, 0.05)) = 0.015
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma require 2darray 
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D_ARRAY(_ShapeMapArray);
            SAMPLER(sampler_ShapeMapArray);

            float _GridScale;
            float _Thickness;
            float _SubdivisionChance;
            
            float4 _ColorBg;
            float4 _ColorTile1;
            float4 _ColorTile2;
            
            float _ShowGrid;
            float4 _GridColor;
            float _BoundsThickness;

            float hash21(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453123);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 scaledUV = input.uv * _GridScale;
                
                // Analytic pixel size fixes the blue smearing artifacts at cell boundaries
                float basePixelSize = length(float2(ddx(scaledUV.x), ddy(scaledUV.y))) * 1.5;

                float dMain[3] = {100.0, 100.0, 100.0};
                float dCorner[3] = {100.0, 100.0, 100.0};
                
                float dim = 1.0; 

                // --- PHASE 1: EVALUATE BOUNDED DISTANCES ---
                [unroll] 
                for (int layer = 0; layer < 3; layer++)
                {
                    float2 currentCellID = floor(scaledUV * dim);
                    float2 currentLocalUV = frac(scaledUV * dim) - 0.5; 

                    [unroll]
                    for (int y = -1; y <= 1; y++)
                    {
                        [unroll]
                        for (int x = -1; x <= 1; x++)
                        {
                            float2 neighborOffset = float2(x, y);
                            float2 neighborCellID = currentCellID + neighborOffset;
                            
                            // Absolute world position to reliably find quadtree parents
                            float2 neighborWorldUV = (neighborCellID + 0.5) / dim;
                            float2 L0_id = floor(neighborWorldUV); 
                            float2 L1_id = floor(neighborWorldUV * 2.0); 
                            
                            bool L0_Active = (hash21(L0_id + 11.11) > _SubdivisionChance);
                            bool L1_Active = (hash21(L1_id + 22.22) > _SubdivisionChance);

                            bool neighborActive = false;
                            if (layer == 0 && L0_Active) neighborActive = true;
                            if (layer == 1 && !L0_Active && L1_Active) neighborActive = true;
                            if (layer == 2 && !L0_Active && !L1_Active) neighborActive = true;

                            if (neighborActive)
                            {
                                float2 neighborLocalUV = currentLocalUV - neighborOffset;
                                float rndShape = hash21(neighborCellID + 44.44);
                                float rndRot = hash21(neighborCellID + 55.55);

                                float2 rotatedUV = neighborLocalUV;
                                if (rndRot < 0.25) rotatedUV = float2(rotatedUV.y, -rotatedUV.x);
                                else if (rndRot < 0.5) rotatedUV = float2(-rotatedUV.x, -rotatedUV.y);
                                else if (rndRot < 0.75) rotatedUV = float2(-rotatedUV.y, rotatedUV.x);

                                uint texIndex = (rndShape < 0.5) ? 0 : 1;
                                float2 texUV = (rotatedUV * 0.5) + 0.5;
                                
                                // LOD 0 fixes the implicit gradient warning inside the loop
                                float2 distRG = SAMPLE_TEXTURE2D_ARRAY_LOD(_ShapeMapArray, sampler_ShapeMapArray, texUV, texIndex, 0).rg;
                                
                                // Apply Thickness
                                float sdfMain = distRG.r - _Thickness;
                                // Corner wings are exactly double the thickness to form the 1/3 radius holes
                                float sdfCorner = distRG.g - (_Thickness * 2.0); 

                                // BOUND TO CELL: This explicitly stops shapes from leaking into neighbors!
                                float squareSDF = max(abs(neighborLocalUV.x), abs(neighborLocalUV.y)) - 0.5;
                                sdfMain = max(sdfMain, squareSDF);
                                sdfCorner = max(sdfCorner, squareSDF);

                                dMain[layer] = min(dMain[layer], sdfMain);
                                dCorner[layer] = min(dCorner[layer], sdfCorner);
                            }
                        }
                    }
                    dim *= 2.0; 
                }

                // --- PHASE 2: RASTERIZE LAYERS ---
                half4 finalColor = _ColorBg;
                float dimOut = 1.0;
                
                [unroll]
                for (int k = 0; k < 3; k++)
                {
                    // Scale local distances to global space
                    float globalSDFMain = dMain[k] / dimOut;
                    float globalSDFCorner = dCorner[k] / dimOut;
                    float layerPixelSize = basePixelSize / dimOut;

                    float4 cMain = (k % 2 == 0) ? _ColorTile1 : _ColorTile2;
                    float4 cWings = (k % 2 == 0) ? _ColorTile2 : _ColorTile1;

                    // Draw Corner Circles (Negative space carve-outs)
                    float maskWings = 1.0 - smoothstep(0.0, layerPixelSize, globalSDFCorner);
                    finalColor = lerp(finalColor, cWings, maskWings * cWings.a);
                    
                    // Draw Main Shape lines
                    float maskMain = 1.0 - smoothstep(0.0, layerPixelSize, globalSDFMain);
                    finalColor = lerp(finalColor, cMain, maskMain * cMain.a);

                    dimOut *= 2.0;
                }

                // --- DEBUG QUADTREE GRID ---
                if (_ShowGrid > 0.5)
                {
                    float2 L0_id = floor(scaledUV);
                    float2 L1_id = floor(scaledUV * 2.0);
                    
                    bool L0_Active = (hash21(L0_id + 11.11) > _SubdivisionChance);
                    bool L1_Active = (hash21(L1_id + 22.22) > _SubdivisionChance);
                    
                    float activeDim = 4.0; 
                    if (L0_Active) activeDim = 1.0; 
                    else if (L1_Active) activeDim = 2.0; 
                    
                    float2 gridUV = frac(scaledUV * activeDim) - 0.5;
                    float edgeDist = max(abs(gridUV.x), abs(gridUV.y));
                    float lineDist = abs(edgeDist - 0.5);
                    
                    // Uses exact thickness with dot pattern
                    float lineMask = 1.0 - smoothstep(_BoundsThickness - basePixelSize, _BoundsThickness + basePixelSize, lineDist);
                    float dashPattern = step(0.5, frac((gridUV.x + gridUV.y) * 20.0));
                    
                    finalColor = lerp(finalColor, _GridColor, lineMask * dashPattern * _GridColor.a);
                }

                return finalColor;
            }
            ENDHLSL
        }
    }
}