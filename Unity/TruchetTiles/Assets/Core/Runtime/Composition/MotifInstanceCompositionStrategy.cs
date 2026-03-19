// TODO ROADMAP:
// [x] Motif instanced composition
// [x] Multi-tileset motif indexing
// [ ] Add frustum culling
// [ ] Add chunked generation
// [ ] Add job system support

using System.Collections.Generic;

namespace Truchet
{
    public class MotifInstanceCompositionStrategy : ITileCompositionStrategy
    {
        public ICompositionResult Compose(
            object layout,
            TileSet[] tileSets,
            int resolution)
        {
            var resource = TileSetGPUResourceManager.Build(tileSets);

            if (resource == null)
                return new InstanceCompositionResult(new List<TileInstanceGPU>(), resolution);

            var offsets = resource.TileSetOffsets;

            List<TileInstanceGPU> instances;

            if (layout is IGridLayout grid)
            {
                var generator =
                    new RegularGridInstanceGenerator(resolution / grid.Width);

                instances = generator.GenerateInstances(grid, tileSets, offsets);
            }
            else if (layout is IHierarchicalTileLayout hierarchical)
            {
                var generator =
                    new QuadTreeInstanceGenerator(resolution);

                instances = generator.GenerateInstances(
                    hierarchical,
                    tileSets,
                    resolution,
                    offsets);
            }
            else
            {
                instances = new List<TileInstanceGPU>();
            }

            return new InstanceCompositionResult(instances, resolution);
        }
    }
}