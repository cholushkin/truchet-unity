// TODO ROADMAP:
// [x] Motif instanced composition
// [x] Multi-tileset motif indexing
// [ ] Add frustum culling
// [ ] Add chunked generation
// [ ] Add job system support

using System.Collections.Generic;

namespace Truchet
{
    public class InstanceComposition : ICompositionStrategy
    {
        public ICompositionResult Compose(
            object layout,
            TileSet[] tileSets,
            int resolution)
        {
            var resource = TileArrayBuilder.Build(tileSets);

            if (resource == null)
                return new InstanceCompositionResult(new List<TileInstanceGPU>(), resolution);

            var offsets = resource.TileSetOffsets;

            List<TileInstanceGPU> instances;

            if (layout is IGridLayout grid)
            {
                var generator =
                    new GridInstanceBuilder(resolution / grid.Width);
                
                // 👉 Composition is doing rendering preparation work. This is slightly wrong layer-wise, BUT acceptable for now.
                // Should be: Composition → abstract result (logical). Rendering → builds GPU instances

                instances = generator.BuildInstances(grid, tileSets, offsets);
            }
            else if (layout is IHierarchicalLayout hierarchical)
            {
                var generator =
                    new QuadTreeInstanceBuilder(resolution);

                instances = generator.BuildInstances(
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