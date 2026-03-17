// TODO ROADMAP:
// [x] Composition abstraction layer
// [ ] Add MarchingSquaresCompositionStrategy
// [ ] Add SDFFieldCompositionStrategy
// [ ] Add mesh-based composition output
// [ ] Add compute-shader driven composition

using System.Collections.Generic;

namespace Truchet
{
    public interface ITileCompositionStrategy
    {
        List<TileInstanceGPU> ComposeInstances(
            object layout, // We temporarily use object layout to allow both IGridLayout and IHierarchicalTileLayout.
            TileSet[] tileSets,
            int resolution);
    }
}