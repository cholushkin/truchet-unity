// TODO ROADMAP:
// [x] Composition abstraction layer
// [x] Return ICompositionResult instead of raw instances
// [ ] Add strongly typed layout interfaces
// [ ] Add async / job support
// [ ] Add chunked composition

using System.Collections.Generic;

namespace Truchet
{
    public interface ITileCompositionStrategy
    {
        ICompositionResult Compose(
            object layout,
            TileSet[] tileSets,
            int resolution);
    }
}