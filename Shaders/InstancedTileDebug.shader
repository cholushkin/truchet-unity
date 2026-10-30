Shader "Truchet/InstancedTileDebug"
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

            struct Attributes
            {
                float3 positionOS : POSITION;
                uint instanceID   : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                TileInstanceGPU instance = _Instances[IN.instanceID];

                float4 worldPos = mul(
                    instance.transform,
                    float4(IN.positionOS, 1.0));

                OUT.positionCS = TransformWorldToHClip(worldPos.xyz);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return half4(0, 1, 0, 1); // bright green
            }

            ENDHLSL
        }
    }
}