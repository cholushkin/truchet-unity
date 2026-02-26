// TODO ROADMAP:
// [x] CPU texture compositing renderer
// [x] Multi-TileSet support
// [ ] Add dirty-region updates
// [ ] Add chunked rendering
// [ ] Add GPU backend swap
// [ ] Add multiscale support
// [ ] Add tile caching

using UnityEngine;

namespace Truchet
{
    public class TextureGridRenderer
    {
        private readonly int _tileResolution;

        public TextureGridRenderer(int tileResolution)
        {
            _tileResolution = tileResolution;
        }

        public Texture2D Render(IGridLayout layout, TileSet[] tileSets, bool debugLines)
        {
            int width = layout.Width * _tileResolution;
            int height = layout.Height * _tileResolution;

            Texture2D output =
                new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;

            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    GridCell cell = layout.GetCell(x, y);

                    if (cell.TileSetId < 0 ||
                        cell.TileSetId >= tileSets.Length)
                        continue;

                    TileSet tileSet = tileSets[cell.TileSetId];

                    if (cell.TileIndex < 0 ||
                        cell.TileIndex >= tileSet.tiles.Length)
                        continue;

                    BlitTile(tileSet.tiles[cell.TileIndex],
                             pixels, width, x, y, cell.Rotation);
                    
                    if (debugLines)
                        DrawCellDebugLines(pixels, width, x, y);
                }
            }

            if (debugLines)
                DrawAxis(pixels, width, height);

            output.SetPixels(pixels);
            output.Apply();

            return output;
        }

        private void BlitTile(
            Tile tile,
            Color[] target,
            int targetWidth,
            int gridX,
            int gridY,
            int rotation)
        {
            if (tile == null || tile.texture == null)
                return;

            Color[] source = tile.texture.GetPixels();

            int startX = gridX * _tileResolution;
            int startY = gridY * _tileResolution;

            for (int y = 0; y < _tileResolution; y++)
            for (int x = 0; x < _tileResolution; x++)
            {
                int srcIndex = GetRotatedIndex(x, y, rotation);

                int tx = startX + x;
                int ty = startY + y;

                target[ty * targetWidth + tx] = source[srcIndex];
            }
        }

        private int GetRotatedIndex(int x, int y, int rotation)
        {
            switch (rotation % 4)
            {
                case 0: return y * _tileResolution + x;
                case 1: return (_tileResolution - 1 - x) * _tileResolution + y;
                case 2: return (_tileResolution - 1 - y) * _tileResolution +
                               (_tileResolution - 1 - x);
                case 3: return x * _tileResolution +
                               (_tileResolution - 1 - y);
                default: return y * _tileResolution + x;
            }
        }
        
        private void DrawAxis(Color[] pixels, int width, int height)
        {
            int size = Mathf.Min(32, width / 6);

            int originX = 4;
            int originY = 4;

            for (int i = 0; i < size; i++)
            {
                int x = originX + i;
                int y = originY;

                if (x >= 0 && x < width && y >= 0 && y < height)
                    pixels[y * width + x] = Color.red;
            }

            for (int i = 0; i < size; i++)
            {
                int x = originX;
                int y = originY + i;

                if (x >= 0 && x < width && y >= 0 && y < height)
                    pixels[y * width + x] = Color.green;
            }
        }
        
        private void DrawCellDebugLines(
            Color[] pixels,
            int targetWidth,
            int gridX,
            int gridY)
        {
            int startX = gridX * _tileResolution;
            int startY = gridY * _tileResolution;

            int rightX = startX + _tileResolution - 1;
            int bottomY = startY + _tileResolution - 1;

            for (int y = startY; y < startY + _tileResolution; y++)
                pixels[y * targetWidth + rightX] = Color.green;

            for (int x = startX; x < startX + _tileResolution; x++)
                pixels[bottomY * targetWidth + x] = Color.green;
        }

    }
}