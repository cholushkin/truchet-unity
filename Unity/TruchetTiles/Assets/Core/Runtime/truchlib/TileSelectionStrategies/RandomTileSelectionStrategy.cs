// TODO ROADMAP:
// [x] Deterministic random selection
// [ ] Add weighted distribution support
// [ ] Add exclusion rules
// [ ] Add adjacency-aware filtering

using System;

namespace Truchet.Tiles
{
    public sealed class RandomTileSelectionStrategy : ITileSelectionStrategy
    {
        private readonly Random random;
        private readonly TileType[] tileTypes;

        public RandomTileSelectionStrategy(int seed)
        {
            random = new Random(seed);
            tileTypes = (TileType[])Enum.GetValues(typeof(TileType));
        }

        public TileType SelectTile(int x, int y, int level)
        {
            // Deterministic per grid position
            int hash = Hash(x, y, level);
            random.Next(); // advance internal state
            int index = Math.Abs(hash ^ random.Next()) % tileTypes.Length;
            return tileTypes[index];
        }

        private int Hash(int x, int y, int level)
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + x;
                h = h * 31 + y;
                h = h * 31 + level;
                return h;
            }
        }
    }
}