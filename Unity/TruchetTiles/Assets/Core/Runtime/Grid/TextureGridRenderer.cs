// TODO ROADMAP:
// [x] CPU texture compositing renderer
// [x] Multi-TileSet support
// [x] Two-pass rendering (classical + winged)
// [x] Winged tile overlap support
// [x] Transparent background (only tiles visible)
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

            // --------------------------------------------------
            // Transparent background
            // --------------------------------------------------

            Color clear = new Color(0, 0, 0, 0);

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = clear;

            // --------------------------------------------------
            // PASS 1 — Classical Tiles
            // --------------------------------------------------

            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    GridCell cell = layout.GetCell(x, y);

                    if (!IsValidCell(cell, tileSets, out Tile tile))
                        continue;

                    if (tile.IsWinged)
                        continue;

                    BlitClassic(tile, pixels, width, x, y, cell.Rotation);
                }
            }

            // --------------------------------------------------
            // PASS 2 — Winged Tiles
            // --------------------------------------------------

            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    GridCell cell = layout.GetCell(x, y);

                    if (!IsValidCell(cell, tileSets, out Tile tile))
                        continue;

                    if (!tile.IsWinged)
                        continue;

                    BlitWinged(tile, pixels, width, height, x, y, cell.Rotation);
                }
            }

            if (debugLines)
                DrawAxis(pixels, width, height);

            output.SetPixels(pixels);
            output.Apply();

            return output;
        }

        // --------------------------------------------------
        // Helpers
        // --------------------------------------------------

        private bool IsValidCell(GridCell cell, TileSet[] tileSets, out Tile tile)
        {
            tile = null;

            if (cell.TileSetId < 0 ||
                cell.TileSetId >= tileSets.Length)
                return false;

            TileSet tileSet = tileSets[cell.TileSetId];

            if (cell.TileIndex < 0 ||
                cell.TileIndex >= tileSet.tiles.Length)
                return false;

            tile = tileSet.tiles[cell.TileIndex];

            if (tile == null || tile.texture == null)
                return false;

            return true;
        }

        // --------------------------------------------------
        // Classical Blit
        // --------------------------------------------------

        private void BlitClassic(
            Tile tile,
            Color[] target,
            int targetWidth,
            int gridX,
            int gridY,
            int rotation)
        {
            Color[] source = tile.texture.GetPixels();

            int startX = gridX * _tileResolution;
            int startY = gridY * _tileResolution;

            for (int y = 0; y < _tileResolution; y++)
            for (int x = 0; x < _tileResolution; x++)
            {
                int srcIndex = GetRotatedIndex(x, y, rotation);

                int tx = startX + x;
                int ty = startY + y;

                Color c = source[srcIndex];

                if (c.a <= 0f)
                    continue;

                target[ty * targetWidth + tx] = c;
            }
        }

        // --------------------------------------------------
        // Winged Blit
        // --------------------------------------------------

        private void BlitWinged(
            Tile tile,
            Color[] target,
            int targetWidth,
            int targetHeight,
            int gridX,
            int gridY,
            int rotation)
        {
            Texture2D tex = tile.texture;
            Color[] source = tex.GetPixels();

            int renderSize = _tileResolution * 2;

            int centerX = gridX * _tileResolution + _tileResolution / 2;
            int centerY = gridY * _tileResolution + _tileResolution / 2;

            int startX = centerX - _tileResolution;
            int startY = centerY - _tileResolution;

            int sourceRes = tex.width;

            for (int y = 0; y < renderSize; y++)
            {
                for (int x = 0; x < renderSize; x++)
                {
                    int tx = startX + x;
                    int ty = startY + y;

                    if (tx < 0 || tx >= targetWidth ||
                        ty < 0 || ty >= targetHeight)
                        continue;

                    int srcX = Mathf.FloorToInt((float)x / renderSize * sourceRes);
                    int srcY = Mathf.FloorToInt((float)y / renderSize * sourceRes);

                    int srcIndex = GetRotatedIndex(srcX, srcY, rotation, sourceRes);

                    Color c = source[srcIndex];

                    if (c.a <= 0f)
                        continue;

                    target[ty * targetWidth + tx] = c;
                }
            }
        }

        // --------------------------------------------------

        private int GetRotatedIndex(int x, int y, int rotation)
        {
            return GetRotatedIndex(x, y, rotation, _tileResolution);
        }

        private int GetRotatedIndex(int x, int y, int rotation, int resolution)
        {
            switch (rotation % 4)
            {
                case 0: return y * resolution + x;
                case 1: return (resolution - 1 - x) * resolution + y;
                case 2: return (resolution - 1 - y) * resolution +
                               (resolution - 1 - x);
                case 3: return x * resolution +
                               (resolution - 1 - y);
                default: return y * resolution + x;
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
    }
}