Shader "Custom/URP_WingedQuadtreeTruchet"
{
    Properties
    {
        [NoScaleOffset] _ShapeMapArray ("Baked SDF Array (RGFloat)", 2DArray) = "white" {}
        _GridScale ("Base Grid Scale", Float) = 4.0
        _SubdivisionChance ("Subdivision Probability", Range(0.0, 1.0)) = 0.6

        [Header(MultiScale Colors)]
        _ColorTile1 ("Primary Color (Ink)", Color) = (0.2, 0.5, 0.9, 1.0)
        _ColorTile2 ("Secondary Color (Paper)", Color) = (0.8, 0.8, 0.8, 1.0)

        [Header(Debug)]
        [Enum(None,0, TileID,1, Ownership,2, Foreground,3, Quadtree,4, Orientation,5, NeighborSource,6)] _DebugMode("Debug Mode", Float) = 0
        [Enum(AllLevels, 255, Level_0, 0, Level_1, 1, Level_2, 2)] _IsolateLevel("Isolate Scale Layer", Float) = 255
        [Toggle] _ShowGrid("Show Grid Overlay", Float) = 0
        _GridColor("Grid Color", Color) = (1.0, 1.0, 0.0, 0.5)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode"="UniversalForward"
            }

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

            float4 _ColorTile1;
            float4 _ColorTile2;
            float _DebugMode;
            float _IsolateLevel;
            float _ShowGrid;
            float4 _GridColor;

            float hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float3 hash33(float3 p3)
            {
                p3 = frac(p3 * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yxz + 33.33);
                return frac((p3.xxy + p3.yzz) * p3.zyx);
            }

            bool isSubdivided(float2 id, int level)
            {
                if (level >= 2) return false;
                return hash12(id + level * 13.37 + 11.11) < _SubdivisionChance;
            }

            bool isTileActive(float2 id, int level)
            {
                if (level == 0)
                {
                    return !isSubdivided(id, 0);
                }
                else if (level == 1)
                {
                    float2 parent = floor(id * 0.5);
                    return isSubdivided(parent, 0) && !isSubdivided(id, 1);
                }
                else
                {
                    float2 parent1 = floor(id * 0.5);
                    float2 parent0 = floor(id * 0.25);
                    return isSubdivided(parent0, 0) && isSubdivided(parent1, 1) && !isSubdivided(id, 2);
                }
            }

            uint getShape(float2 id, int level)
            {
                return hash12(id + level * 13.37 + 44.44) < 0.5 ? 0 : 1;
            }

            uint getRotation(float2 id, int level)
            {
                return (uint)(hash12(id + level * 13.37 + 55.55) * 4.0);
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
                float2 scaledUV = input.uv * _GridScale;

                float d[3] = {1000.0, 1000.0, 1000.0}; // Foreground
                float d2[3] = {1000.0, 1000.0, 1000.0}; // Ownership

                float bestLevel = -1.0;
                float2 bestID = 0;
                uint bestRot = 0;

                [unroll]
                for (int L = 0; L <= 2; L++)
                {
                    if (_IsolateLevel >= 0.0 && _IsolateLevel < 3.0 && L != (int)_IsolateLevel) continue;

                    float scale = pow(2.0, L);
                    float tileSize = 1.0 / scale;

                    // Shift grid center for 2x2 neighbor check
                    float2 centerID = floor(scaledUV * scale + 0.5);

                    float layerFg = 1000.0;
                    float layerOwn = 1000.0;
                    float2 layerID = centerID;
                    uint layerRot = 0;

                    // 2x2 neighbor sweep instead of 3x3
                    [unroll]
                    for (int y = -1; y <= 0; y++)
                    {
                        [unroll]
                        for (int x = -1; x <= 0; x++)
                        {
                            float2 neighborID = centerID + float2(x, y);

                            if (!isTileActive(neighborID, L)) continue;

                            uint shape = getShape(neighborID, L);
                            uint rot = getRotation(neighborID, L);

                            float2 centerWorld = (neighborID + 0.5) * tileSize;
                            float2 localUV = (scaledUV - centerWorld) / tileSize;

                            float2 rotUV = localUV;
                            if (rot == 1) rotUV = float2(localUV.y, -localUV.x);
                            else if (rot == 2) rotUV = float2(-localUV.x, -localUV.y);
                            else if (rot == 3) rotUV = float2(-localUV.y, localUV.x);

                            float2 texUV = rotUV * 0.5 + 0.5;
                            float2 sdfs = SAMPLE_TEXTURE2D_ARRAY_LOD(_ShapeMapArray, sampler_ShapeMapArray, texUV, shape, 0).rg;

                            float ownDist = sdfs.g * tileSize;
                            float fgDist = sdfs.r * tileSize;

                            if (ownDist < layerOwn)
                            {
                                layerOwn = ownDist;
                                layerID = neighborID;
                                layerRot = rot;
                            }
                            layerFg = min(layerFg, fgDist);
                        }
                    }

                    d[L] = layerFg;
                    d2[L] = layerOwn;

                    if (layerOwn <= 0.0)
                    {
                        bestLevel = L;
                        bestID = layerID;
                        bestRot = layerRot;
                    }
                }

                // --- TOPOLOGICAL CSG COMPOSITING ---
                float globalSDF = -d[0];
                globalSDF = max(d2[0], globalSDF);
                globalSDF = min(max(globalSDF, -d2[1]), d[1]);
                globalSDF = max(min(globalSDF, d2[2]), -d[2]);

                // --- DEBUG VISUALIZATIONS ---
                if (_DebugMode == 1) return half4(hash33(float3(bestID.x, bestID.y, bestLevel)), 1.0);
                if (_DebugMode == 3) return half4(globalSDF < 0 ? float3(0.1, 0.8, 0.3) : float3(0.8, 0.1, 0.3), 1.0) * (1.0 - exp(-20.0 * abs(globalSDF)));
                if (_DebugMode == 4) return half4(bestLevel == 0 ? float3(1, 0, 0) : (bestLevel == 1 ? float3(0, 1, 0) : float3(0, 0, 1)), 1.0);
                if (_DebugMode == 5) return half4(bestRot == 0 ? float3(1, 1, 1) : (bestRot == 1 ? float3(1, 0, 0) : (bestRot == 2 ? float3(0, 1, 0) : float3(0, 0, 1))), 1.0);
                if (_DebugMode == 6)
                {
                    float2 centerWorld = (bestID + 0.5) / pow(2.0, max(bestLevel, 0.0));
                    float2 delta = centerWorld - scaledUV;
                    return half4(float3(delta.x * 2.0 + 0.5, delta.y * 2.0 + 0.5, 0.5), 1.0);
                }

                // --- FINAL RASTERIZATION ---
                float pixelSize = fwidth(scaledUV.x) * 1.5;

                float shapeMask = 1.0 - smoothstep(-pixelSize, pixelSize, globalSDF);
                half4 finalColor = lerp(_ColorTile1, _ColorTile2, shapeMask);

                // --- GRID OVERLAY ---
                if (_ShowGrid > 0.5 && bestLevel >= 0.0)
                {
                    float2 tileUV = frac(scaledUV * pow(2.0, bestLevel));
                    float2 distToEdge = min(tileUV, 1.0 - tileUV);
                    float minEdge = min(distToEdge.x, distToEdge.y);

                    float gridThickness = fwidth(minEdge) * 1.5;
                    float gridMask = 1.0 - smoothstep(0.0, gridThickness, minEdge);

                    finalColor = lerp(finalColor, _GridColor, gridMask * _GridColor.a);
                }

                return finalColor;
            }
            ENDHLSL
        }
    }
}