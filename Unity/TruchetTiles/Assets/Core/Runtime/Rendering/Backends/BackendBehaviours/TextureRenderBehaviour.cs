using UnityEngine;
using System.Collections.Generic;

namespace Truchet
{
    // TODO ROADMAP:
    // [x] Texture backend integration
    // [x] Sampling mode support
    // [x] Cache toggle exposure
    // [x] Binary threshold toggle
    // [ ] Live updates without full rebuild
    // [ ] Debug visualization
    // [ ] Async rendering

    public class TextureRenderBehaviour : TruchetRenderBehaviour
    {
        [Header("Output")]
        public int Resolution = 512;

        [Header("Sampling")]
        public TileSamplingMode SamplingMode = TileSamplingMode.Coverage;

        [Header("Performance")]
        public bool UseTilePixelCache = true;

        [Header("Post Processing")]
        public bool ApplyBinaryThreshold = true; // NEW

        [Header("Output")]
        public Color BackgroundColor = Color.white;

        public Renderer TargetRenderer;

        private TextureRenderBackend _backend = new TextureRenderBackend();

        public override void Render(
            List<TileInstance> instances,
            TileSet[] tileSets,
            int gridWidth)
        {
            var options = new TileRenderOptions
            {
                BackgroundColor = BackgroundColor,
                SamplingMode = SamplingMode,
                UseTilePixelCache = UseTilePixelCache,
                ApplyBinaryThreshold = ApplyBinaryThreshold // NEW
            };

            _backend.SetOptions(options);

            Texture2D tex = _backend.Render(
                instances,
                tileSets,
                Resolution);

            if (TargetRenderer != null)
            {
                TargetRenderer.sharedMaterial.mainTexture = tex;
            }
        }
    }
}