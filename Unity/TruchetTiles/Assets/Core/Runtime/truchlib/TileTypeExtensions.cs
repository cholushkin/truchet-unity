// TODO ROADMAP:
// [x] Encapsulate connectivity bitmask logic
// [x] Provide directional query helpers
// [x] Provide mask extraction helper
// [ ] Add rotation utilities
// [ ] Add adjacency compatibility helpers
// [ ] Support metadata-based tile definitions

using System;

namespace Truchet.Tiles
{
    public static class TileTypeExtensions
    {
        private const int DirectionMask = 0b_1111;

        /// <summary>
        /// Extracts the directional connectivity mask.
        /// </summary>
        public static Direction GetDirections(this TileType type)
        {
            return (Direction)((int)type & DirectionMask);
        }

        /// <summary>
        /// Checks if tile has a connection in given direction.
        /// </summary>
        public static bool HasDirection(this TileType type, Direction direction)
        {
            return (type.GetDirections() & direction) != 0;
        }

        /// <summary>
        /// Returns tile identity index (upper bits).
        /// </summary>
        public static int GetTileId(this TileType type)
        {
            return ((int)type >> 4);
        }

        /// <summary>
        /// Returns true if two tiles are directionally compatible.
        /// </summary>
        public static bool IsCompatibleWith(
            this TileType a,
            TileType b,
            Direction directionFromAToB)
        {
            if (!a.HasDirection(directionFromAToB))
                return false;

            Direction opposite = GetOpposite(directionFromAToB);

            return b.HasDirection(opposite);
        }

        public static Direction GetOpposite(Direction direction)
        {
            return direction switch
            {
                Direction.North => Direction.South,
                Direction.South => Direction.North,
                Direction.East  => Direction.West,
                Direction.West  => Direction.East,
                _ => Direction.None
            };
        }
    }
}