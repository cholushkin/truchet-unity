Shader "Custom/QuadtreeTruchet_URP"
{
    Properties
    {
        [Header(Colors)]
        _Color1 ("Spectrum Target", Color) = (0.7, 1.4, 0.4, 1.0)
        _Color2 ("Pink Target 1", Color) = (1.0, 0.1, 0.2, 1.0)
        _Color3 ("Pink Target 2", Color) = (1.0, 0.1, 0.5, 1.0)
        _Color4 ("Pink Base", Color) = (0.1, 0.02, 0.06, 1.0)
        
        [Header(Geometry Controls)]
        _TileScale ("Tile Scale", Float) = 5.0
        _LineThickness ("Line Thickness Inverse", Range(1.5, 10.0)) = 3.0
        _CornerRadius ("Corner Dot Radius", Range(0.0, 2.0)) = 1.0
        _Seed ("Random Generation Seed", Float) = 0.0
        
        [Header(Toggles)]
        [Toggle] _BoundaryCaps ("Enable Boundary Caps (Creates Artifacts)", Float) = 0
        [Toggle] _ShowCornerDots ("Show Corner Dots", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define COLOR 1

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

            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float4 _Color4;
            
            float _TileScale;
            float _LineThickness;
            float _CornerRadius;
            float _Seed;
            float _BoundaryCaps;
            float _ShowCornerDots;

            // Seedable hash function
            float2 hash22(float2 p) 
            { 
                float n = sin(dot(p, float2(57.0 + _Seed, 27.0 - _Seed)));
                return frac(float2(262144.0, 32768.0) * n);
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
                float2 uv = input.uv - 0.5;
                float2 oP = uv * _TileScale;    

                float4 d = float4(1e5, 1e5, 1e5, 1e5);
                float4 d2 = float4(1e5, 1e5, 1e5, 1e5);
                
                float2 rndTh[3] = { float2(0.5, 0.35), float2(0.5, 0.7), float2(0.5, 1.0) };
                float dim = 1.0;
                
                [unroll]
                for(int k = 0; k < 3; k++)
                {
                    float2 ip = floor(oP * dim);
                         
                    for(int j = -1; j <= 1; j++)
                    {
                        for(int i = -1; i <= 1; i++)
                        {
                            float2 rndIJ = hash22(ip + float2(i, j));
                            float2 rndIJ2 = hash22(floor((ip + float2(i, j)) / 2.0));
                            float2 rndIJ4 = hash22(floor((ip + float2(i, j)) / 4.0));
                            
                            if(k == 1 && rndIJ2.y < rndTh[0].y) continue;
                            if(k == 2 && (rndIJ2.y < rndTh[1].y || rndIJ4.y < rndTh[0].y)) continue;
                          
                            if(rndIJ.y < rndTh[k].y)
                            {
                                float2 p = oP - (ip + 0.5 + float2(i, j)) / dim;
                                float square = max(abs(p.x), abs(p.y)) - 0.5 / dim; 
                                
                                if(rndIJ.x < rndTh[k].x) p = p.yx;
                                if(frac(rndIJ.x * 57.543 + 0.37) < rndTh[k].x) p.x = -p.x;
                                
                                float c, c2;
                                
                                // Primary Truchet Arc
                                c = abs(length(p - float2(-0.5, 0.5) / dim) - 0.5 / dim) - 0.5 / _LineThickness / dim;

                                // Secondary Truchet Arc / Caps
                                if(frac(rndIJ.x * 157.763 + 0.49) > 0.35)
                                {
                                    c2 = abs(length(p - float2(0.5, -0.5) / dim) - 0.5 / dim) - 0.5 / _LineThickness / dim;
                                }
                                else
                                {  
                                    c2 = length(p - float2(0.5, 0.0) / dim) - 0.5 / _LineThickness / dim;
                                    c2 = min(c2, length(p - float2(0.0, -0.5) / dim) - 0.5 / _LineThickness / dim);
                                }

                                float truchet = min(c, c2);
                                float c3 = 1e5;
                                
                                // The artifact culprit - Disabled by default
                                if (_BoundaryCaps > 0.5) 
                                {
                                    float2 p2 = abs(float2(p.y - p.x, p.x + p.y) * 0.7071) - float2(0.5, 0.5) * 0.7071 / dim;
                                    c3 = length(p2) - 0.5 / _LineThickness / dim;
                                }

                                c = min(c3, max(square, truchet));
                                d[k] = min(d[k], c); 
                                
                                // Large corner circles
                                if (_ShowCornerDots > 0.5)
                                {
                                    float2 pCorner = abs(p) - 0.5 / dim;
                                    float l = length(pCorner);
                                    float cCorner = min(l - (1.0 / _LineThickness / dim) * _CornerRadius, square);
                                    d2[k] = min(d2[k], cCorner); 
                                }
                            }
                        }
                    }
                    dim *= 2.0;
                }
                
                float3 col = float3(0.25, 0.25, 0.25);
                float fo = fwidth(uv.y) * 5.0; 
                float pat3 = clamp(sin((oP.x - oP.y) * 6.283 * 40.0) * 1.0 + 0.9, 0.0, 1.0) * 0.25 + 0.75;
                
                float3 pCol1 = float3(1.0, 1.0, 1.0);
                float3 pCol2 = float3(0.125, 0.125, 0.125);    
                
                #if COLOR == 1
                    pCol1 = _Color1.rgb; 
                #elif COLOR == 2
                    pCol1 = lerp(_Color2.rgb, _Color3.rgb, uv.y * 0.5 + 0.5);
                    pCol2 = _Color4.rgb; 
                #endif
                
                d.x = max(d2.x, -d.x);
                d.x = min(max(d.x, -d2.y), d.y);
                d.x = max(min(d.x, d2.z), -d.z);

                float pat = clamp(-sin(d.x * 6.283 * 20.0), 0.0, 1.0);
                float pat2 = clamp(sin(d.x * 6.283 * 16.0) * 1.0 + 0.9, 0.0, 1.0) * 0.3 + 0.7;
                float sh = clamp(0.75 + d.x * 2.0, 0.0, 1.0);

                #if COLOR == 1
                    col *= pat;
                    d.x = -(d.x + 0.03);

                    col = lerp(col, float3(0,0,0), (1.0 - smoothstep(0.0, fo * 5.0, d.x)));
                    col = lerp(col, float3(0,0,0), 1.0 - smoothstep(0.0, fo, d.x));
                    col = lerp(col, float3(0.8, 1.2, 0.6), 1.0 - smoothstep(0.0, fo * 2.0, d.x + 0.02));
                    col = lerp(col, float3(0,0,0), 1.0 - smoothstep(0.0, fo * 2.0, d.x + 0.03));
                    col = lerp(col, float3(0.7, 1.4, 0.4) * pat2, 1.0 - smoothstep(0.0, fo * 2.0, d.x + 0.05));
                    col *= sh; 
                #else
                    col = pCol1;
                    col = lerp(col, float3(0,0,0), (1.0 - smoothstep(0.0, fo * 5.0, d.x)) * 0.35);
                    col = lerp(col, float3(0,0,0), 1.0 - smoothstep(0.0, fo, d.x));
                    col = lerp(col, pCol2, 1.0 - smoothstep(0.0, fo, d.x + 0.02));
                    col *= pat3; 
                #endif
                
                col *= max(1.15 - length(uv) * 0.5, 0.0);
                
                #if COLOR == 1
                    col = lerp(col, col.yxz, uv.y * 0.75 + 0.5); 
                    col = lerp(col, col.zxy, uv.x * 0.7 + 0.5); 
                #endif
                
                return half4(sqrt(max(col, 0.0)), 1.0);
            }
            ENDHLSL
        }
    }
}