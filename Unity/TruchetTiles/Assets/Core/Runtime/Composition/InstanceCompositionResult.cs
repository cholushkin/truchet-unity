// TODO ROADMAP:
// [x] Instance-based composition result (logical instances)
// [ ] Add bounds metadata
// [ ] Add chunk support
// [ ] Add GPU buffer conversion helpers
// [ ] Add LOD metadata
// [ ] Add tile set usage metadata

using System.Collections.Generic;

namespace Truchet
{
    public class InstanceCompositionResult : ICompositionResult
    {
        public List<TileInstance> Instances;

        public int Resolution;

        public InstanceCompositionResult(
            List<TileInstance> instances,
            int resolution)
        {
            Instances = instances;
            Resolution = resolution;
        }
    }
}