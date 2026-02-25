// TODO ROADMAP:
// [x] Simple NESW topology struct
// [ ] Add helper methods
// [ ] Add rotation helpers
// [ ] Add compatibility check

using UnityEngine;

namespace Truchet.TileCooking
{
    [System.Serializable]
    public struct TileTopology
    {
        public bool north;
        public bool east;
        public bool south;
        public bool west;

        public int Mask =>
            (north ? 1 : 0) |
            (east  ? 2 : 0) |
            (south ? 4 : 0) |
            (west  ? 8 : 0);
    }
}