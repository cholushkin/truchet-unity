Shader "Truchet/GPUInstancedTile_Stab"
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

            // Must match C# struct layout exactly (80 bytes)
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

                OUT.positionCS = mul(UNITY_MATRIX_VP, worldPos);

                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Solid bright green
                return float4(0, 1, 0, 1);
            }

            ENDHLSL
        }
    }
}