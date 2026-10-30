Shader "Custom/TileThreeColorOutline_URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _ColorA ("Color A", Color) = (0,0,0,1)
        _ColorB ("Color B", Color) = (1,1,1,1)
        _Background ("Background", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0,5)) = 1.0
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
            float4 _Background;

            float4 _OutlineColor;
            float  _OutlineThickness;

            float4 _MainTex_TexelSize;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half SampleAlpha(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                half t = tex.r;
                half a = tex.a;

                // --- tile color ---
                half3 tileRGB = lerp(_ColorA.rgb, _ColorB.rgb, t);
                half tileA    = lerp(_ColorA.a,  _ColorB.a,  t) * a;

                // --- edge detection (alpha gradient) ---
                float2 texel = _MainTex_TexelSize.xy * _OutlineThickness;

                half aL = SampleAlpha(i.uv - float2(texel.x, 0));
                half aR = SampleAlpha(i.uv + float2(texel.x, 0));
                half aD = SampleAlpha(i.uv - float2(0, texel.y));
                half aU = SampleAlpha(i.uv + float2(0, texel.y));

                half edge = abs(a - aL) + abs(a - aR) + abs(a - aD) + abs(a - aU);
                edge = saturate(edge * 2.0);

                // --- mix outline ---
                half outlineA = _OutlineColor.a * edge;
                half3 outlineRGB = _OutlineColor.rgb;

                // outline over tile
                half finalTileA = outlineA + tileA * (1.0h - outlineA);
                half3 finalTileRGB =
                    (outlineRGB * outlineA +
                     tileRGB * tileA * (1.0h - outlineA)) / max(finalTileA, 1e-5);

                // --- background ---
                half3 bgRGB = _Background.rgb;
                half bgA    = _Background.a;

                half outA = finalTileA + bgA * (1.0h - finalTileA);

                half3 outRGB =
                    (finalTileRGB * finalTileA +
                     bgRGB * bgA * (1.0h - finalTileA)) / max(outA, 1e-5);

                return half4(outRGB, outA);
            }

            ENDHLSL
        }
    }
}