// TODO ROADMAP:
// [x] Basic rectangular grid implementation
// [x] Renamed from RegularGridLayout
// [ ] Add neighbor query API
// [ ] Add iteration helpers
// [ ] Add deterministic fill utilities
// [ ] Add resize support

namespace Truchet
{
    public class RegularGridTileMap : IGridLayout
    {
        private readonly GridCell[,] _cells;

        public int Width { get; }
        public int Height { get; }

        public RegularGridTileMap(int width, int height)
        {
            Width = width;
            Height = height;

            _cells = new GridCell[width, height];

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                _cells[x, y] = new GridCell(x, y);
        }

        public bool IsValid(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public GridCell GetCell(int x, int y)
        {
            if (!IsValid(x, y))
                throw new System.ArgumentOutOfRangeException();

            return _cells[x, y];
        }

        public void SetTile(int x, int y, int tileSetId, int tileIndex, int rotation)
        {
            if (!IsValid(x, y))
                return;

            GridCell cell = _cells[x, y];
            cell.TileSetId = tileSetId;
            cell.TileIndex = tileIndex;
            cell.Rotation = rotation;

            _cells[x, y] = cell;
        }
    }
}