Shader "Custom/URP_WingedDevGrid"
{
    Properties
    {
        [NoScaleOffset] _ShapeMapArray ("Winged Skeleton Array", 2DArray) = "white" {}
        
        [Header(Grid Settings)]
        _Columns ("Grid Columns", Int) = 2
        _Rows ("Grid Rows", Int) = 2
        _TileCount ("Total Tiles in Array", Int) = 2
        
        // The mathematical constant for Carlson Winged tiles is exactly 1/6 (0.166666).
        // This specific radius ensures the wings form perfect tangent circles at the corners.
        _Thickness ("Line Thickness (Carlson Constant = 0.1666)", Range(0.01, 0.35)) = 0.166666
        
        [Header(Colors)]
        _ColorBg ("Background Color (Alpha Supported)", Color) = (0.85, 0.85, 0.85, 0.0)
        _ColorTile2 ("Corner Wings Color (Layer 2)", Color) = (0.3, 0.6, 0.9, 1.0)
        _ColorTile1 ("Main Shape Color (Layer 1)", Color) = (0.05, 0.05, 0.05, 1.0)
        
        [Header(Debug Boundaries)]
        [Toggle] _ShowBounds ("Show Nominal Cell Boundaries", Float) = 1.0
        _BoundsColor ("Boundary Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _BoundsThickness ("Boundary Thickness", Range(0.001, 0.05)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
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

            int _Columns;
            int _Rows;
            int _TileCount;
            float _Thickness;
            
            float4 _ColorBg;
            float4 _ColorTile1;
            float4 _ColorTile2;
            
            float _ShowBounds;
            float4 _BoundsColor;
            float _BoundsThickness;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                // Flip UV Y-axis if necessary depending on your mesh orientation
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Grid UV Mapping
                // Multiply UVs by the number of columns/rows * 2.0. 
                // The * 2.0 ensures each grid slot is large enough to contain the [-1.0, 1.0] extended canvas, 
                // effectively skipping exactly 1 cell of space between the nominal boundaries of each tile.
                float2 scaledUV = input.uv * float2(_Columns * 2.0, _Rows * 2.0);
                
                // Determine which column and row we are currently in
                int col = floor(scaledUV.x / 2.0);
                int row = floor(scaledUV.y / 2.0);
                
                // Calculate the array index (looping safely if we draw more grid slots than we have tiles)
                int tileIndex = (col + (row * _Columns)) % max(1, _TileCount);

                // 2. Local Tile UV Mapping
                // The center of each 2.0-sized grid slot is at (1.0, 1.0), (3.0, 1.0), etc.
                float2 centerID = float2(col * 2.0 + 1.0, row * 2.0 + 1.0);
                
                // This produces local UVs precisely in the [-1.0, 1.0] range around the center of each tile!
                float2 localUV = scaledUV - centerID; 

                // Map local [-1.0, 1.0] to texture space [0.0, 1.0] for the sampler
                float2 texUV = localUV * 0.5 + 0.5;

                // 3. Sample the Dual-Channel SDF (R = Main, G = Wings)
                float2 distRG = SAMPLE_TEXTURE2D_ARRAY(_ShapeMapArray, sampler_ShapeMapArray, texUV, tileIndex).rg;

                // 4. Expand skeletons by the Carlson constant thickness
                float sdfMain = distRG.r - _Thickness;
                float sdfCorners = distRG.g - _Thickness;

                // 5. Anti-Aliased Rasterization
                float pixelSize1 = fwidth(sdfMain);
                float maskMain = 1.0 - smoothstep(0.0, pixelSize1 * 1.5, sdfMain);

                float pixelSize2 = fwidth(sdfCorners);
                float maskCorners = 1.0 - smoothstep(0.0, pixelSize2 * 1.5, sdfCorners);

                // 6. Alpha Blending Colors
                half4 finalColor = _ColorBg;
                
                // Draw Layer 2 (Wings) over Background
                finalColor = lerp(finalColor, _ColorTile2, maskCorners * _ColorTile2.a);
                // Draw Layer 1 (Main) over Layer 2
                finalColor = lerp(finalColor, _ColorTile1, maskMain * _ColorTile1.a);

                // 7. Debug Nominal Boundaries
                if (_ShowBounds > 0.5)
                {
                    // The nominal boundary is exactly at ±0.5 in our localUV space
                    float edgeDist = max(abs(localUV.x), abs(localUV.y));
                    float lineDist = abs(edgeDist - 0.5);
                    
                    float linePixelSize = fwidth(lineDist);
                    float lineMask = 1.0 - smoothstep(_BoundsThickness - linePixelSize, _BoundsThickness + linePixelSize, lineDist);
                    
                    // Dashed line pattern
                    float dashPattern = step(0.5, frac((localUV.x + localUV.y) * 20.0));
                    lineMask *= dashPattern;

                    finalColor = lerp(finalColor, _BoundsColor, lineMask * _BoundsColor.a);
                }

                return finalColor;
            }
            ENDHLSL
        }
    }
}