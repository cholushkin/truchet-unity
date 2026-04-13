Shader "Truchet/UI/Tilemap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        // Choose which color is transparent
        // (0,0,0) = black transparent
        // (1,1,1) = white transparent
        _TransparentColor ("Transparent Color", Color) = (1,1,1,1)

        // Final rendered color
        _TintColor ("Tint Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
        }

        Pass
        {
            Name "UI"
            Tags { "LightMode"="UniversalForward" }

            // Standard alpha blending (no premultiply needed anymore)
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _TransparentColor;
            float4 _TintColor;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Grayscale value (your texture is effectively grayscale already)
                float gray = tex.r;

                // Determine mode (white or black transparent)
                float target = _TransparentColor.r;

                float alpha;

                if (target > 0.5)
                {
                    // White = transparent
                    alpha = 1.0 - gray;
                }
                else
                {
                    // Black = transparent
                    alpha = gray;
                }

                alpha = saturate(alpha);

                // IMPORTANT: ignore texture color completely
                float3 color = _TintColor.rgb;

                return float4(color, alpha);
            }

            ENDHLSL
        }
    }
}