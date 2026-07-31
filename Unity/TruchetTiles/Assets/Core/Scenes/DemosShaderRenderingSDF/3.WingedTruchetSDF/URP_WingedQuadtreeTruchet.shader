Shader "Custom/URP_WingedQuadtreeTruchet"
{
    Properties
    {
        [NoScaleOffset] _ShapeMapArray ("Baked SDF Array (RGFloat)", 2DArray) = "white" {}
        _GridScale ("Base Grid Scale", Float) = 4.0
        _SubdivisionChance ("Subdivision Probability", Range(0.0, 1.0)) = 0.6
        
        [Header(MultiScale Colors)]
        _ColorBg ("Void Color (Outside Ownership)", Color) = (1.0, 0.0, 1.0, 1.0)
        _ColorTile1 ("Primary Color", Color) = (0.2, 0.5, 0.9, 1.0) 
        _ColorTile2 ("Secondary Color", Color) = (0.05, 0.05, 0.05, 1.0) 
        
        [Header(Debug)]
        [Enum(None,0, TileID,1, Ownership,2, Foreground,3, Quadtree,4, Orientation,5, NeighborSource,6)] _DebugMode("Debug Mode", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma require 2darray 
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            TEXTURE2D_ARRAY(_ShapeMapArray);
            SAMPLER(sampler_ShapeMapArray);

            float _GridScale;
            float _SubdivisionChance;
            
            float4 _ColorBg;
            float4 _ColorTile1;
            float4 _ColorTile2;
            float _DebugMode;

            // Deterministic hash functions
            float hash12(float2 p) {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float3 hash33(float3 p3) {
                p3 = frac(p3 * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yxz + 33.33);
                return frac((p3.xxy + p3.yzz) * p3.zyx);
            }

            // --- CANONICAL IDENTIFIER LOGIC ---

            bool isSubdivided(float2 id, int level) {
                if (level >= 2) return false;
                return hash12(id + level * 13.37 + 11.11) < _SubdivisionChance;
            }

            bool isTileActive(float2 id, int level) {
                if (level == 0) {
                    return !isSubdivided(id, 0);
                } else if (level == 1) {
                    float2 parent = floor(id * 0.5);
                    return isSubdivided(parent, 0) && !isSubdivided(id, 1);
                } else {
                    float2 parent1 = floor(id * 0.5);
                    float2 parent0 = floor(id * 0.25);
                    return isSubdivided(parent0, 0) && isSubdivided(parent1, 1) && !isSubdivided(id, 2);
                }
            }

            uint getShape(float2 id, int level) {
                return hash12(id + level * 13.37 + 44.44) < 0.5 ? 0 : 1;
            }

            uint getRotation(float2 id, int level) {
                return (uint)(hash12(id + level * 13.37 + 55.55) * 4.0);
            }

            struct Candidate {
                float fg;
                float own;
                int level;
                float2 id;
                uint shape;
                uint rot;
                int parity;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 scaledUV = input.uv * _GridScale;
                
                Candidate best;
                best.own = 1000.0;
                best.fg = 1000.0;
                best.level = -1;

                // --- QUADTREE NEIGHBOR EVALUATION ---
                [unroll]
                for (int L = 0; L <= 2; L++) 
                {
                    float scale = pow(2.0, L);
                    float tileSize = 1.0 / scale;
                    float2 centerID = floor(scaledUV * scale);
                    
                    [unroll]
                    for (int y = -1; y <= 1; y++) 
                    {
                        [unroll]
                        for (int x = -1; x <= 1; x++) 
                        {
                            float2 neighborID = centerID + float2(x, y);
                            
                            if (!isTileActive(neighborID, L)) continue;
                            
                            uint shape = getShape(neighborID, L);
                            uint rot = getRotation(neighborID, L);
                            
                            // Map to Tile Local Domain [-1, 1]
                            float2 centerWorld = (neighborID + 0.5) * tileSize;
                            float2 localUV = (scaledUV - centerWorld) / tileSize; 
                            localUV *= 2.0; 
                            
                            // Deterministic Orientation Transform
                            float2 rotUV = localUV;
                            if (rot == 1) rotUV = float2(localUV.y, -localUV.x);
                            else if (rot == 2) rotUV = float2(-localUV.x, -localUV.y);
                            else if (rot == 3) rotUV = float2(-localUV.y, localUV.x);
                            
                            float2 texUV = rotUV * 0.5 + 0.5;
                            float2 sdfs = SAMPLE_TEXTURE2D_ARRAY_LOD(_ShapeMapArray, sampler_ShapeMapArray, texUV, shape, 0).rg;
                            
                            // Convert back to global coordinates
                            float worldFg = sdfs.r * tileSize;
                            float worldOwn = sdfs.g * tileSize;
                            
                            // --- OWNERSHIP COMPOSITING ---
                            bool is_better = false;
                            if (worldOwn <= 0.0) 
                            {
                                if (best.own > 0.0) {
                                    is_better = true; // Claim empty space
                                } else {
                                    // Finer level overrides coarser parent ownership completely
                                    if (L > best.level) is_better = true;
                                    else if (L == best.level && worldOwn < best.own) is_better = true;
                                }
                            } 
                            else if (best.own > 0.0 && worldOwn < best.own) 
                            {
                                // Track closest in case we are in unbounded void
                                is_better = true;
                            }
                            
                            // Coupled Replacement
                            if (is_better) 
                            {
                                best.fg = worldFg;
                                best.own = worldOwn;
                                best.level = L;
                                best.id = neighborID;
                                best.shape = shape;
                                best.rot = rot;
                                best.parity = L % 2;
                            }
                        }
                    }
                }

                // --- DEBUG VISUALIZATIONS ---
                if (_DebugMode == 1) return half4(hash33(float3(best.id.x, best.id.y, best.level)), 1.0);
                if (_DebugMode == 2) return half4(best.own < 0 ? float3(0.1, 0.5, 0.8) : float3(0.9, 0.2, 0.2), 1.0) * (1.0 - exp(-20.0 * abs(best.own)));
                if (_DebugMode == 3) return half4(best.fg < 0 ? float3(0.1, 0.8, 0.3) : float3(0.8, 0.1, 0.3), 1.0) * (1.0 - exp(-20.0 * abs(best.fg)));
                if (_DebugMode == 4) return half4(best.level == 0 ? float3(1,0,0) : (best.level == 1 ? float3(0,1,0) : float3(0,0,1)), 1.0);
                if (_DebugMode == 5) return half4(best.rot == 0 ? float3(1,1,1) : (best.rot == 1 ? float3(1,0,0) : (best.rot == 2 ? float3(0,1,0) : float3(0,0,1))), 1.0);
                if (_DebugMode == 6) 
                {
                    float2 centerWorld = (best.id + 0.5) / pow(2.0, best.level);
                    float2 delta = centerWorld - scaledUV;
                    return half4(float3(delta.x * 2.0 + 0.5, delta.y * 2.0 + 0.5, 0.5), 1.0);
                }

                // --- FINAL RASTERIZATION ---
                if (best.own > 0.0) return _ColorBg; // Outside all tiles

                float4 colorInk = (best.parity == 0) ? _ColorTile1 : _ColorTile2;
                float4 colorPaper = (best.parity == 0) ? _ColorTile2 : _ColorTile1;
                
                float pixelSize = fwidth(best.fg);
                float mask = 1.0 - smoothstep(0.0, pixelSize * 1.5, best.fg);
                
                return lerp(colorPaper, colorInk, mask);
            }
            ENDHLSL
        }
    }
}