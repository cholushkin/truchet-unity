// TODO ROADMAP:
// [x] Instance-based composition result
// [ ] Add bounds metadata
// [ ] Add chunk support
// [ ] Add GPU buffer conversion helpers
// [ ] Add LOD metadata
// [ ] Add tile set usage metadata

using System.Collections.Generic;

namespace Truchet
{
    /// <summary>
    /// Composition result for GPU instanced rendering.
    ///
    /// Contains:
    /// - Tile instance data (CPU-side)
    ///
    /// Renderer will convert this into GPU buffers.
    /// </summary>
    public class InstanceCompositionResult : ICompositionResult
    {
        public List<TileInstanceGPU> Instances;

        public int Resolution;

        public InstanceCompositionResult(
            List<TileInstanceGPU> instances,
            int resolution)
        {
            Instances = instances;
            Resolution = resolution;
        }
    }
}