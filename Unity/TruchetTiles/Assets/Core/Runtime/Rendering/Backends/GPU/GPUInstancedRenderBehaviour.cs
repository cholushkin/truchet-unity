// TODO ROADMAP (real plan):
// [x] Split build vs render lifecycle
// [x] Store GPU instance buffer once
// [x] Continuous rendering in Update()
// [x] Cache TileArray per TileSet combination
// [x] Normalize → pixel conversion via builder
// [ ] Support dynamic instance updates (partial rebuild)
// [ ] Add frustum culling (CPU-side first)
// [ ] Add chunked rendering support

using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    public class GPUInstancedRenderBehaviour : TruchetRenderBehaviour
    {
        [Header("GPU")]
        [SerializeField] private Material _material;
        [SerializeField] private float _scale = 1f;

        private GPUInstancedRenderBackend _backend;

        private List<TileInstanceGPU> _gpuInstances;

        private float _currentResolution;

        // Cached TileArray
        private TileSetGPUResource _cachedResource;
        private int _cachedTileSetHash;

        private void Awake()
        {
            if (_material != null)
                _backend = new GPUInstancedRenderBackend(_material);
        }

        public override void Render(
            List<TileInstance> instances,
            TileSet[] tileSets,
            int gridWidth)
        {
            if (_backend == null)
            {
                Debug.LogError("GPU backend not initialized.");
                return;
            }

            // Resolution (pixel space scale)
            _currentResolution = gridWidth * _scale;

            // --------------------------------------------------
            // TileArray (cached)
            // --------------------------------------------------

            int hash = ComputeTileSetHash(tileSets);

            if (_cachedResource == null || hash != _cachedTileSetHash)
            {
                _cachedResource = TileArrayBuilder.Build(tileSets);
                _cachedTileSetHash = hash;

                if (_cachedResource == null)
                {
                    Debug.LogError("Failed to build TileArray.");
                    return;
                }

                _backend.SetTileTextureArray(_cachedResource.TextureArray);
            }

            // --------------------------------------------------
            // Instance build (NOW WITH SCALING)
            // --------------------------------------------------

            var builder = new InstanceRenderDataBuilder();

            _gpuInstances = builder.Build(
                instances,
                _cachedResource.TileSetOffsets,
                _currentResolution);
        }

        private void Update()
        {
            if (_backend == null || _gpuInstances == null || _gpuInstances.Count == 0)
                return;

            _backend.RenderInstances(
                _gpuInstances,
                Mathf.RoundToInt(_currentResolution));
        }

        // --------------------------------------------------
        // HASHING
        // --------------------------------------------------

        private int ComputeTileSetHash(TileSet[] tileSets)
        {
            unchecked
            {
                int hash = 17;

                if (tileSets == null)
                    return hash;

                hash = hash * 31 + tileSets.Length;

                foreach (var set in tileSets)
                {
                    if (set == null)
                    {
                        hash = hash * 31;
                        continue;
                    }

                    hash = hash * 31 + set.GetInstanceID();

                    if (set.tiles == null)
                        continue;

                    hash = hash * 31 + set.tiles.Length;

                    foreach (var tile in set.tiles)
                    {
                        if (tile == null)
                        {
                            hash = hash * 31;
                            continue;
                        }

                        hash = hash * 31 + tile.GetInstanceID();

                        if (tile.texture != null)
                            hash = hash * 31 + tile.texture.GetInstanceID();
                    }
                }

                return hash;
            }
        }
    }
}