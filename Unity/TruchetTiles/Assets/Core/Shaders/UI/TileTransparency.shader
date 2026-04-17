Shader "Custom/TileThreeColor_URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _ColorA ("Color A (black)", Color) = (0,0,0,1)
        _ColorB ("Color B (white)", Color) = (1,1,1,1)
        _Background ("Background", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Forward"
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
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _ColorA;
            float4 _ColorB;
            float4 _Background;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half t = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).r;

                half3 tileRGB = lerp(_ColorA.rgb, _ColorB.rgb, t);
                half tileA    = lerp(_ColorA.a,  _ColorB.a,  t);

                half3 bgRGB = _Background.rgb;
                half bgA    = _Background.a;

                half outA = tileA + bgA * (1.0h - tileA);

                half3 outRGB = 0;
                if (outA > 0.0h)
                {
                    outRGB =
                        (tileRGB * tileA +
                         bgRGB * bgA * (1.0h - tileA)) / outA;
                }

                return half4(outRGB, outA);
            }

            ENDHLSL
        }
    }
}