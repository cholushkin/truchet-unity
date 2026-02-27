// TODO ROADMAP:
// [x] Simple NESW topology struct
// [x] Mask encoding
// [x] Rotation helper
// [ ] Add compatibility check
// [ ] Add neighbor matching helpers


namespace Truchet
{
    [System.Serializable]
    public struct TileTopology
    {
        public bool north;
        public bool east;
        public bool south;
        public bool west;

        /// <summary>
        /// Bitmask encoding:
        /// bit 0 → north
        /// bit 1 → east
        /// bit 2 → south
        /// bit 3 → west
        /// </summary>
        public int Mask =>
            (north ? 1 : 0) |
            (east  ? 2 : 0) |
            (south ? 4 : 0) |
            (west  ? 8 : 0);

        /// <summary>
        /// Rotates NESW mask clockwise by 90° steps.
        /// rotation: can be negative or greater than 3.
        /// </summary>
        public static int RotateMask(int mask, int rotation)
        {
            // Normalize rotation to 0..3
            rotation = ((rotation % 4) + 4) % 4;

            for (int i = 0; i < rotation; i++)
            {
                int north = (mask & 1) != 0 ? 1 : 0;
                int east  = (mask & 2) != 0 ? 1 : 0;
                int south = (mask & 4) != 0 ? 1 : 0;
                int west  = (mask & 8) != 0 ? 1 : 0;

                mask =
                    (west  << 0) | // new north
                    (north << 1) | // new east
                    (east  << 2) | // new south
                    (south << 3);  // new west
            }

            return mask;
        }
    }
}