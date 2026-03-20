Shader "Truchet/InstancedTile"
{
    SubShader
    {
        Tags { 
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct TileInstanceGPU
            {
                float4x4 transform;
                uint motifIndex;
                uint level;
                uint pad0;
                uint pad1;
            };

            StructuredBuffer<TileInstanceGPU> _Instances;

            TEXTURE2D_ARRAY(_TileArray);
            SAMPLER(sampler_TileArray);

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                uint instanceID   : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float motifIndex  : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                TileInstanceGPU instance = _Instances[IN.instanceID];

                float4 worldPos = mul(
                    instance.transform,
                    float4(IN.positionOS, 1.0));

                OUT.positionCS = TransformWorldToHClip(worldPos.xyz);

                OUT.uv = IN.uv;
                OUT.motifIndex = (float)instance.motifIndex;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return SAMPLE_TEXTURE2D_ARRAY(
                    _TileArray,
                    sampler_TileArray,
                    IN.uv,
                    IN.motifIndex);
            }

            ENDHLSL
        }
    }
}