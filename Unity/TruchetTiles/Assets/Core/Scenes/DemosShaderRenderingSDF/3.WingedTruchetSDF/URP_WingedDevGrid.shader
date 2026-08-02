Shader "Custom/URP_WingedDevGrid"
{
    Properties
    {
        [NoScaleOffset] _ShapeMapArray ("Winged Skeleton Array", 2DArray) = "white" {}
        
        [Header(Grid Settings)]
        _Columns ("Grid Columns", Int) = 2
        _Rows ("Grid Rows", Int) = 2
        _TileCount ("Total Tiles in Array", Int) = 2
        
        // Expanded range for testing boundaries. 
        // 0.166666 represents the baseline baked into the SDF (no offset).
        _Thickness ("Line Thickness (Base = 0.1666)", Range(0.0, 1.0)) = 0.166666
        
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
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 scaledUV = input.uv * float2(_Columns * 2.0, _Rows * 2.0);
                
                int col = floor(scaledUV.x / 2.0);
                int row = floor(scaledUV.y / 2.0);
                
                int tileIndex = (col + (row * _Columns)) % max(1, _TileCount);

                float2 centerID = float2(col * 2.0 + 1.0, row * 2.0 + 1.0);
                float2 localUV = scaledUV - centerID; 
                float2 texUV = localUV * 0.5 + 0.5;

                float2 distRG = SAMPLE_TEXTURE2D_ARRAY(_ShapeMapArray, sampler_ShapeMapArray, texUV, tileIndex).rg;

                // Calculate the offset from the baked 1/6 constant
                float thicknessOffset = _Thickness - 0.166666;
                
                // Shift the SDF boundary by the offset
                float sdfMain = distRG.r - thicknessOffset;
                float sdfCorners = distRG.g - thicknessOffset;

                float pixelSize1 = fwidth(sdfMain);
                float maskMain = 1.0 - smoothstep(0.0, pixelSize1 * 1.5, sdfMain);

                float pixelSize2 = fwidth(sdfCorners);
                float maskCorners = 1.0 - smoothstep(0.0, pixelSize2 * 1.5, sdfCorners);

                half4 finalColor = _ColorBg;
                finalColor = lerp(finalColor, _ColorTile2, maskCorners * _ColorTile2.a);
                finalColor = lerp(finalColor, _ColorTile1, maskMain * _ColorTile1.a);

                if (_ShowBounds > 0.5)
                {
                    float edgeDist = max(abs(localUV.x), abs(localUV.y));
                    float lineDist = abs(edgeDist - 0.5);
                    
                    float linePixelSize = fwidth(lineDist);
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