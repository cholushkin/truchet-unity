Shader "Truchet/UI/TileOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _TransparentColor ("Transparent Color", Color) = (1,1,1,1)

        _FillColor ("Fill Color", Color) = (0,0,0,1)
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)

        _OutlineThickness ("Outline Thickness (pixels)", Float) = 1.0
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
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _TransparentColor;
            float4 _FillColor;
            float4 _OutlineColor;
            float _OutlineThickness;

            float4 _MainTex_TexelSize; // (1/width, 1/height, width, height)

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float GetMask(float2 uv)
            {
                float gray = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).r;

                // white transparent OR black transparent
                if (_TransparentColor.r > 0.5)
                    return 1.0 - gray;
                else
                    return gray;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                float mask = GetMask(uv);

                // --- sample neighbors ---
                float2 texel = _MainTex_TexelSize.xy * _OutlineThickness;

                float neighbor =
                    GetMask(uv + float2(texel.x, 0)) +
                    GetMask(uv - float2(texel.x, 0)) +
                    GetMask(uv + float2(0, texel.y)) +
                    GetMask(uv - float2(0, texel.y));

                neighbor = saturate(neighbor);

                // --- detect outline ---
                float outline = saturate(neighbor - mask);

                // --- final color ---
                float3 color =
                    _FillColor.rgb * mask +
                    _OutlineColor.rgb * outline;

                float alpha = saturate(mask + outline);

                return float4(color, alpha);
            }

            ENDHLSL
        }
    }
}