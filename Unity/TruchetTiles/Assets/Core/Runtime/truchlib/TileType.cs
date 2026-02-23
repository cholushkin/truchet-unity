// TODO ROADMAP:
// [x] Reintroduce Direction bitmask
// [x] Reintroduce TileType enum
// [ ] Add helper utilities for connectivity queries
// [ ] Move to Truchet.Core namespace (future split)
// [ ] Add metadata mapping layer

using System;

namespace Truchet.Tiles
{
    [Flags]
    public enum Direction
    {
        None  = 0b_0000,
        North = 0b_0001,
        East  = 0b_0010,
        South = 0b_0100,
        West  = 0b_1000
    }

    public enum TileType
    {
        Empty           = (0  << 4) | Direction.None,

        Vertical        = (1  << 4) | Direction.North | Direction.South,
        Horizontal      = (2  << 4) | Direction.East  | Direction.West,
        Cross           = (3  << 4) | Direction.North | Direction.East  | Direction.South | Direction.West,

        ForwardSlash    = (4  << 4) | Direction.North | Direction.East  | Direction.South | Direction.West,
        BackSlash       = (5  << 4) | Direction.North | Direction.East  | Direction.South | Direction.West,

        Frown_NW        = (6  << 4) | Direction.North | Direction.West,
        Frown_NE        = (7  << 4) | Direction.North | Direction.East,
        Frown_SE        = (8  << 4) | Direction.South | Direction.East,
        Frown_SW        = (9  << 4) | Direction.South | Direction.West,

        T_N             = (10 << 4) | Direction.North | Direction.East  | Direction.West,
        T_E             = (11 << 4) | Direction.North | Direction.East  | Direction.South,
        T_S             = (12 << 4) | Direction.East  | Direction.South | Direction.West,
        T_W             = (13 << 4) | Direction.North | Direction.South | Direction.West
    }
}