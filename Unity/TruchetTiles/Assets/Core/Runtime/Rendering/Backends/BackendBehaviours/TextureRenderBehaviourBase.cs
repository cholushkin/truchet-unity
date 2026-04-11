using UnityEngine;
using System.Collections.Generic;

namespace Truchet
{
    // TODO ROADMAP:
    // [x] Extract common texture rendering logic
    // [x] Backend + options centralized
    // [x] Abstract output application
    // [ ] Support multiple targets
    // [ ] Async rendering

    public abstract class TextureRenderBehaviourBase : TruchetRenderBehaviour
    {
        [Header("Output")]
        public int Resolution = 512;

        [Header("Sampling")]
        public TileSamplingMode SamplingMode = TileSamplingMode.Coverage;

        [Header("Performance")]
        public bool UseTilePixelCache = true;

        [Header("Post Processing")]
        public bool ApplyBinaryThreshold = true;

        [Header("Output")]
        public Color BackgroundColor = Color.white;

        protected TextureRenderBackend _backend = new TextureRenderBackend();

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
                ApplyBinaryThreshold = ApplyBinaryThreshold
            };

            _backend.SetOptions(options);

            Texture2D tex = _backend.Render(
                instances,
                tileSets,
                Resolution);

            ApplyTexture(tex);
        }

        protected abstract void ApplyTexture(Texture2D texture);
    }
}