Shader "Custom/URP_MultishapeTruchetSDF"
{
    Properties
    {
        _GridScale ("Grid Scale", Float) = 10.0
        _Thickness ("Line Thickness", Range(0.01, 0.4)) = 0.1
        _ColorBg ("Background Color", Color) = (0.1, 0.1, 0.1, 1)
        _ColorFg ("Shape Color", Color) = (0.3, 0.6, 0.9, 1)
        
        [Toggle] _ShowGrid ("Show Debug Grid", Float) = 0.0
        _GridColor ("Grid Color", Color) = (1.0, 0.0, 0.0, 1.0)
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

            float _GridScale;
            float _Thickness;
            float4 _ColorBg;
            float4 _ColorFg;
            float _ShowGrid;
            float4 _GridColor;

            // Pseudo-random function
            float hash21(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // --- SHAPE LIBRARY FUNCTIONS ---
            
            float ShapeArcs(float2 uv, float halfThick)
            {
                float dist1 = length(uv - float2(-0.5, -0.5)) - 0.5;
                float dist2 = length(uv - float2(0.5, 0.5)) - 0.5;
                return min(abs(dist1) - halfThick, abs(dist2) - halfThick);
            }

            float ShapeCross(float2 uv, float halfThick)
            {
                float dist1 = abs(uv.x) - halfThick;
                float dist2 = abs(uv.y) - halfThick;
                return min(dist1, dist2);
            }

            // -------------------------------

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
                float2 cellID = floor(scaledUV);
                float2 localUV = frac(scaledUV) - 0.5; 

                float rndShape = hash21(cellID);
                float rndRot = hash21(cellID + float2(123.45, 678.90));

                // 90-Degree Rotations
                if (rndRot < 0.25) localUV = float2(localUV.y, -localUV.x);
                else if (rndRot < 0.5) localUV = float2(-localUV.x, -localUV.y);
                else if (rndRot < 0.75) localUV = float2(-localUV.y, localUV.x);

                // Determine Shape
                float truchetSDF = 0.0;
                float halfThick = _Thickness * 0.5;

                if (rndShape < 0.6) 
                {
                    truchetSDF = ShapeArcs(localUV, halfThick);
                }
                else 
                {
                    truchetSDF = ShapeCross(localUV, halfThick);
                }

                // Rasterize Truchet
                float pixelSize = fwidth(truchetSDF);
                float shapeMask = 1.0 - smoothstep(0.0, pixelSize * 1.5, truchetSDF);
                half4 finalColor = lerp(_ColorBg, _ColorFg, shapeMask);

                // Debug Grid Overlay
                if (_ShowGrid > 0.5)
                {
                    // Find distance to the closest edge of the cell (0.5 is the boundary)
                    float gridEdgeDist = max(abs(localUV.x), abs(localUV.y));
                    
                    // Create a sharp 1-pixel line using fwidth
                    float gridPixelSize = fwidth(gridEdgeDist);
                    float gridMask = smoothstep(0.5 - (gridPixelSize * 2.0), 0.5, gridEdgeDist);
                    
                    finalColor = lerp(finalColor, _GridColor, gridMask);
                }

                return finalColor;
            }
            ENDHLSL
        }
    }
}