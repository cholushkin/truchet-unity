// TODO ROADMAP:
// [x] Deterministic grid generation
// [x] Strategy-driven tile selection
// [ ] Add multi-level subdivision support
// [ ] Add adjacency constraint solving
// [ ] Add streaming / chunk generation
// [ ] Add metadata output model

using System;

namespace Truchet.Tiles
{
    public sealed class TileGridBuilder
    {
        private readonly GenerationSettings settings;
        private readonly ITileSelectionStrategy strategy;

        public TileGridBuilder(
            GenerationSettings settings,
            ITileSelectionStrategy strategy)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        /// <summary>
        /// Builds a deterministic 2D grid of TileType.
        /// </summary>
        public TileType[,] Build()
        {
            int rows = settings.Rows;
            int columns = settings.Columns;

            var grid = new TileType[rows, columns];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    grid[y, x] = strategy.SelectTile(x, y, 0);
                }
            }

            return grid;
        }
    }
}