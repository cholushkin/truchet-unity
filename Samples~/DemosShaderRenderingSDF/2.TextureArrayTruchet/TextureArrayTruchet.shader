Shader "Custom/TextureArrayTruchet"
{
    Properties
    {
        [NoScaleOffset] _ShapeMapArray ("Shape Texture Array (2DArray)", 2DArray) = "white" {}
        _GridScale ("Grid Scale", Float) = 10.0
        
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
            // Ensure the hardware supports Texture2DArray
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

            // Texture Array Definitions
            TEXTURE2D_ARRAY(_ShapeMapArray);
            SAMPLER(sampler_ShapeMapArray);

            float _GridScale;
            float4 _ColorBg;
            float4 _ColorFg;
            float _ShowGrid;
            float4 _GridColor;

            // Pseudo-random function
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
                float2 cellID = floor(scaledUV);
                
                // Texture sampling expects UVs from [0, 1]
                float2 localUV = frac(scaledUV); 

                float rndShape = hash21(cellID);
                float rndRot = hash21(cellID + float2(123.45, 678.90));

                // 90-Degree Rotations around the center (0.5, 0.5)
                localUV -= 0.5;
                if (rndRot < 0.25) 
                {
                    localUV = float2(localUV.y, -localUV.x);
                }
                else if (rndRot < 0.5) 
                {
                    localUV = float2(-localUV.x, -localUV.y);
                }
                else if (rndRot < 0.75) 
                {
                    localUV = float2(-localUV.y, localUV.x);
                }
                localUV += 0.5;

                // Determine Shape Index 
                // Index 0: Truchet Arcs | Index 1: Intersecting Cross
                uint texIndex = (rndShape < 0.6) ? 0 : 1;

                // Sample the specific layer of the Texture2DArray
                // Assuming grayscale mask textures where white (1.0) is the shape
                float shapeMask = SAMPLE_TEXTURE2D_ARRAY(_ShapeMapArray, sampler_ShapeMapArray, localUV, texIndex).r;

                // Color mapping
                half4 finalColor = lerp(_ColorBg, _ColorFg, shapeMask);

                // Debug Grid Overlay
                if (_ShowGrid > 0.5)
                {
                    float2 centeredUV = frac(scaledUV) - 0.5;
                    float gridEdgeDist = max(abs(centeredUV.x), abs(centeredUV.y));
                    
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