Shader "Custom/TileThreeBand_Hard_URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _ColorA ("Color A", Color) = (0,0,0,1)
        _ColorB ("Color B", Color) = (1,1,1,1)
        _ColorC ("Color C (Middle)", Color) = (1,0,0,1)

        _Background ("Background", Color) = (1,1,1,1)

        _Threshold1 ("Threshold 1", Range(0,1)) = 0.33
        _Threshold2 ("Threshold 2", Range(0,1)) = 0.66
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
            float4 _ColorC;
            float4 _Background;

            float _Threshold1;
            float _Threshold2;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                half t = tex.r;
                half a = tex.a;

                half3 tileRGB;
                half tileA;

                if (t < _Threshold1)
                {
                    tileRGB = _ColorA.rgb;
                    tileA   = _ColorA.a;
                }
                else if (t < _Threshold2)
                {
                    tileRGB = _ColorC.rgb;
                    tileA   = _ColorC.a;
                }
                else
                {
                    tileRGB = _ColorB.rgb;
                    tileA   = _ColorB.a;
                }

                tileA *= a;

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