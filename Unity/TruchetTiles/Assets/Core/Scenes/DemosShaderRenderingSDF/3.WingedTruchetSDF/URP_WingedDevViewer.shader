Shader "Custom/URP_WingedDevViewer"
{
    Properties
    {
        [NoScaleOffset] _ShapeMapArray ("Winged Skeleton Array", 2DArray) = "white" {}
        _TileIndex ("Tile Index (0=Slash, 1=Plus)", Int) = 0
        
        _Thickness ("Line Thickness", Range(0.01, 0.35)) = 0.166 
        
        [Header(Colors)]
        _ColorBg ("Background Color (Alpha Supported)", Color) = (0.85, 0.85, 0.85, 0.0)
        _ColorTile2 ("Corner Wings Color", Color) = (0.3, 0.6, 0.9, 1.0)
        _ColorTile1 ("Main Shape Color", Color) = (0.05, 0.05, 0.05, 1.0)
        
        [Header(Debug Boundaries)]
        [Toggle] _ShowBounds ("Show Nominal Cell Boundary", Float) = 1.0
        _BoundsColor ("Boundary Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _BoundsThickness ("Boundary Thickness", Range(0.001, 0.05)) = 0.01
    }
    SubShader
    {
        // Set up standard Alpha Blending for transparent UI/Backgrounds
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

            int _TileIndex;
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
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample both channels simultaneously
                float2 distRG = SAMPLE_TEXTURE2D_ARRAY(_ShapeMapArray, sampler_ShapeMapArray, input.uv, _TileIndex).rg;

                // Expand both skeletons by thickness
                float sdfMain = distRG.r - _Thickness;
                float sdfCorners = distRG.g - _Thickness;

                // Rasterize Main Shape (Tile 1)
                float pixelSize1 = fwidth(sdfMain);
                float maskMain = 1.0 - smoothstep(0.0, pixelSize1 * 1.5, sdfMain);

                // Rasterize Corner Wings (Tile 2)
                float pixelSize2 = fwidth(sdfCorners);
                float maskCorners = 1.0 - smoothstep(0.0, pixelSize2 * 1.5, sdfCorners);

                // --- ALPHA BLENDING ---
                half4 finalColor = _ColorBg;
                
                // Layer 2 on top of Background
                finalColor = lerp(finalColor, _ColorTile2, maskCorners * _ColorTile2.a);
                
                // Layer 1 on top of Layer 2
                finalColor = lerp(finalColor, _ColorTile1, maskMain * _ColorTile1.a);


                // --- DEBUG NOMINAL BOUNDARIES ---
                if (_ShowBounds > 0.5)
                {
                    float2 localUV = input.uv * 2.0 - 1.0; 
                    float edgeDist = max(abs(localUV.x), abs(localUV.y));
                    float lineDist = abs(edgeDist - 0.5);
                    
                    float linePixelSize = fwidth(lineDist);
                    
                    // Uses an explicit, zoom-independent thickness rather than 0.0
                    float lineMask = 1.0 - smoothstep(_BoundsThickness - linePixelSize, _BoundsThickness + linePixelSize, lineDist);
                    
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