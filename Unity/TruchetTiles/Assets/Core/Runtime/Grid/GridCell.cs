// TODO ROADMAP:
// [x] Basic grid cell data container
// [x] Rotation support (0-3)
// [x] TileSetId support (multi-tileset rendering)
// [ ] Add metadata storage
// [ ] Add flags (blocked, reserved, etc.)
// [ ] Add multiscale support hook

namespace Truchet
{
    /// <summary>
    /// Logical grid cell.
    /// Pure topology container.
    /// </summary>
    public struct GridCell
    {
        public int X;
        public int Y;

        public int TileSetId;  
        public int TileIndex;
        public int Rotation;    // 0,1,2,3 (90° steps)

        public GridCell(int x, int y)
        {
            X = x;
            Y = y;
            TileSetId = -1;
            TileIndex = -1;
            Rotation = 0;
        }
    }
}