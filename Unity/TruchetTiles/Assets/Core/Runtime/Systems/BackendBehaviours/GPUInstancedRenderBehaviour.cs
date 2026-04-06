// TODO ROADMAP (real plan):
// [x] Split build vs render lifecycle
// [x] Store GPU instance buffer once
// [x] Continuous rendering in Update()
// [ ] Avoid rebuilding TileArray every Generate()
// [ ] Cache TileArray per TileSet combination
// [ ] Support dynamic instance updates (partial rebuild)
// [ ] Add frustum culling (CPU-side first)
// [ ] Add chunked rendering support

// TODO IDEAS (brainstorm):
// - GPU frustum + occlusion culling (compute shader)
// - Indirect draw args buffer reuse
// - LOD system (different tile densities)
// - GPU animation support (tile transforms over time)
// - Debug overlay (draw bounds / instance count)
// - Multi-material support (blend tile sets)
// - Editor preview mode (render in edit mode)

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

        // --------------------------------------------------
        // INIT
        // --------------------------------------------------

        private void Awake()
        {
            if (_material != null)
                _backend = new GPUInstancedRenderBackend(_material);
        }

        // --------------------------------------------------
        // BUILD (called by runtime)
        // --------------------------------------------------

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

            // Build texture array
            var resource = TileArrayBuilder.Build(tileSets);

            if (resource == null)
            {
                Debug.LogError("Failed to build TileArray.");
                return;
            }

            _backend.SetTileTextureArray(resource.TextureArray);

            // Build GPU instance data
            var builder = new InstanceRenderDataBuilder();

            _gpuInstances = builder.Build(
                instances,
                resource.TileSetOffsets);

            // Store resolution for rendering
            _currentResolution = gridWidth * _scale;
        }

        // --------------------------------------------------
        // RENDER LOOP (IMPORTANT)
        // --------------------------------------------------

        private void Update()
        {
            if (_backend == null || _gpuInstances == null || _gpuInstances.Count == 0)
                return;

            _backend.RenderInstances(
                _gpuInstances,
                Mathf.RoundToInt(_currentResolution));
        }
    }
}