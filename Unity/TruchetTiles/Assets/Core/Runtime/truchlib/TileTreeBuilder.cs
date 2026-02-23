// TODO ROADMAP:
// [x] Implement recursive multi-scale subdivision
// [x] Strategy-driven tile selection per level
// [ ] Add probabilistic subdivision control
// [ ] Add adjacency constraint propagation
// [ ] Add partial rebuild support
// [ ] Add chunk streaming support
// TODO: Animated subdivision
// TODO: LOD rendering
// TODO: Progressive reveal
// TODO: GPU instancing per level
// TODO: Adaptive subdivision
// TODO: Metadata tagging
using System;

namespace Truchet.Tiles
{
    public sealed class TileTreeBuilder
    {
        private readonly GenerationSettings settings;
        private readonly ITileSelectionStrategy strategy;

        public TileTreeBuilder(
            GenerationSettings settings,
            ITileSelectionStrategy strategy)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        public TileNode[,] Build()
        {
            int rows = settings.Rows;
            int columns = settings.Columns;

            var grid = new TileNode[rows, columns];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    grid[y, x] = BuildNode(x, y, 0);
                }
            }

            return grid;
        }

        private TileNode BuildNode(int x, int y, int level)
        {
            if (level >= settings.Levels - 1)
            {
                var tile = strategy.SelectTile(x, y, level);
                return new LeafTileNode(x, y, level, tile);
            }

            // Subdivide into 4 quadrants
            int nextLevel = level + 1;

            var children = new TileNode[4];

            children[0] = BuildNode(x * 2,     y * 2,     nextLevel); // NW
            children[1] = BuildNode(x * 2 + 1, y * 2,     nextLevel); // NE
            children[2] = BuildNode(x * 2 + 1, y * 2 + 1, nextLevel); // SE
            children[3] = BuildNode(x * 2,     y * 2 + 1, nextLevel); // SW

            return new ContainerTileNode(x, y, level, children);
        }
    }
}