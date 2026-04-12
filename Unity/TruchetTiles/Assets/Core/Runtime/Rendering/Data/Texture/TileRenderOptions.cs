using UnityEngine;

namespace Truchet
{
    // TODO ROADMAP:
    // [x] Sampling mode abstraction
    // [x] Optional tile pixel caching
    // [x] Optional binary threshold
    // [ ] Add blending modes
    // [ ] Add debug visualization
    // [ ] Add edge handling modes

    public struct TileRenderOptions
    {
        public Color BackgroundColor;

        public TileSamplingMode SamplingMode;

        public bool UseTilePixelCache;

        public bool ApplyBinaryThreshold;
    }
}