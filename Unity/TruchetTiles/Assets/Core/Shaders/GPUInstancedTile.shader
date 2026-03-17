Shader "Truchet/GPUInstancedTile"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct TileInstanceGPU
            {
                float4x4 transform;
                uint motifIndex;
                uint level;
            };

            StructuredBuffer<TileInstanceGPU> _Instances;

            UNITY_DECLARE_TEX2DARRAY(_TileArray);

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
                uint motifIndex   : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                TileInstanceGPU instance =
                    _Instances[IN.instanceID];

                float4 worldPos =
                    mul(instance.transform,
                        float4(IN.positionOS, 1.0));

                OUT.positionCS =
                    mul(UNITY_MATRIX_VP, worldPos);

                OUT.uv = IN.uv;
                OUT.motifIndex = instance.motifIndex;

                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                return UNITY_SAMPLE_TEX2DARRAY(
                    _TileArray,
                    float3(IN.uv, IN.motifIndex));
            }

            ENDHLSL
        }
    }
}