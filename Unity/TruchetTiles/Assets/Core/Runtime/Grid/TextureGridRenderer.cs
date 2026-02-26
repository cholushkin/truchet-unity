// TODO ROADMAP:
// [x] CPU texture compositing renderer
// [x] Rotation support (0, 90, 180, 270)
// [ ] Add dirty-region updates
// [ ] Add chunked rendering
// [ ] Add GPU backend swap
// [ ] Add multiscale support
// [ ] Add tile caching (avoid repeated GetPixels allocations)

using UnityEngine;
using Truchet.Tiles;

namespace Truchet.Grid
{
    /// <summary>
    /// Renders entire grid into a single Texture2D.
    /// Deterministic CPU compositing backend.
    /// </summary>
    public class TextureGridRenderer
    {
        private readonly TileSet _tileSet;
        private readonly int _tileResolution;

        public TextureGridRenderer(TileSet tileSet, int tileResolution)
        {
            _tileSet = tileSet;
            _tileResolution = tileResolution;
        }

        /// <summary>
        /// Renders the provided layout into a single texture.
        /// </summary>
        public Texture2D Render(IGridLayout layout)
        {
            if (layout == null)
            {
                Debug.LogError("TextureGridRenderer: Layout is null.");
                return null;
            }

            if (_tileSet == null || _tileSet.tiles == null)
            {
                Debug.LogError("TextureGridRenderer: TileSet is not assigned.");
                return null;
            }

            int width = layout.Width * _tileResolution;
            int height = layout.Height * _tileResolution;

            Texture2D output = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            FillBackground(pixels);

            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    GridCell cell = layout.GetCell(x, y);

                    if (!IsValidTileIndex(cell.TileIndex))
                        continue;

                    BlitTile(pixels, width, x, y, cell.TileIndex, cell.Rotation);
                }
            }

            output.SetPixels(pixels);
            output.Apply();

            return output;
        }

        #region Internal

        private void FillBackground(Color[] pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
        }

        private bool IsValidTileIndex(int index)
        {
            return index >= 0 && index < _tileSet.tiles.Length;
        }

        private void BlitTile(
            Color[] target,
            int targetWidth,
            int gridX,
            int gridY,
            int tileIndex,
            int rotation)
        {
            Tile tile = _tileSet.tiles[tileIndex];

            if (tile == null || tile.texture == null)
            {
                Debug.LogError($"No tile in tileset with index {tileIndex}");
                return;
            }

            Texture2D texture = tile.texture;

            if (texture.width != _tileResolution || texture.height != _tileResolution)
            {
                Debug.LogError("Tile resolution mismatch.");
                return;
            }

            Color[] source = texture.GetPixels();

            int startX = gridX * _tileResolution;
            int startY = gridY * _tileResolution;

            for (int y = 0; y < _tileResolution; y++)
            {
                for (int x = 0; x < _tileResolution; x++)
                {
                    int srcIndex = GetRotatedIndex(x, y, rotation);
                    int tx = startX + x;
                    int ty = startY + y;

                    int dstIndex = ty * targetWidth + tx;

                    target[dstIndex] = source[srcIndex];
                }
            }
        }

        private int GetRotatedIndex(int x, int y, int rotation)
        {
            switch (rotation % 4)
            {
                case 0: // 0°
                    return y * _tileResolution + x;

                case 1: // 90°
                    return (_tileResolution - 1 - x) * _tileResolution + y;

                case 2: // 180°
                    return (_tileResolution - 1 - y) * _tileResolution +
                           (_tileResolution - 1 - x);

                case 3: // 270°
                    return x * _tileResolution +
                           (_tileResolution - 1 - y);

                default:
                    return y * _tileResolution + x;
            }
        }

        #endregion
    }
}
