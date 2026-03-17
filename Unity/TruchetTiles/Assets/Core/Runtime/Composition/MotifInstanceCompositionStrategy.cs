// TODO ROADMAP:
// [x] Motif instanced composition
// [ ] Add SDF-aware motif mapping
// [ ] Add frustum culling support
// [ ] Add burst-compatible path
// [ ] Add chunked generation

using System.Collections.Generic;

namespace Truchet
{
    public class MotifInstanceCompositionStrategy : ITileCompositionStrategy
    {
        public List<TileInstanceGPU> ComposeInstances(
            object layout,
            TileSet[] tileSets,
            int resolution)
        {
            if (layout is IGridLayout grid)
            {
                var generator =
                    new RegularGridInstanceGenerator(resolution / grid.Width);

                return generator.GenerateInstances(grid, tileSets);
            }

            if (layout is IHierarchicalTileLayout hierarchical)
            {
                var generator =
                    new QuadTreeInstanceGenerator(resolution);

                return generator.GenerateInstances(
                    hierarchical,
                    tileSets,
                    resolution);
            }

            return new List<TileInstanceGPU>();
        }
    }
}