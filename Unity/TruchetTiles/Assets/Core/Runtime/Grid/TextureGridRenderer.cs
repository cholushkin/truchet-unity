// TODO ROADMAP:
// [x] CPU texture compositing renderer
// [x] Rotation support (0, 90, 180, 270)
// [x] TileSet sourced from layout
// [ ] Add dirty-region updates
// [ ] Add chunked rendering
// [ ] Add GPU backend swap
// [ ] Add multiscale support
// [ ] Add tile caching (avoid repeated GetPixels allocations)

using UnityEngine;

namespace Truchet
{
    /// <summary>
    /// Renders entire grid into a single Texture2D.
    /// Deterministic CPU compositing backend.
    /// </summary>
    public class TextureGridRenderer
    {
        private readonly int _tileResolution;

        public TextureGridRenderer(int tileResolution)
        {
            _tileResolution = tileResolution;
        }

        /// <summary>
        /// Renders the provided layout into a single texture.
        /// </summary>
        public Texture2D Render(RegularGridLayout layout)
        {
            if (layout == null)
            {
                Debug.LogError("TextureGridRenderer: Layout is null.");
                return null;
            }

            if (layout.TileSet == null || layout.TileSet.tiles == null)
            {
                Debug.LogError("TextureGridRenderer: TileSet is not assigned in layout.");
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

                    if (!IsValidTileIndex(layout, cell.TileIndex))
                        continue;

                    BlitTile(layout, pixels, width, x, y, cell.TileIndex, cell.Rotation);
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

        private bool IsValidTileIndex(RegularGridLayout layout, int index)
        {
            return index >= 0 && index < layout.TileSet.tiles.Length;
        }

        private void BlitTile(
            RegularGridLayout layout,
            Color[] target,
            int targetWidth,
            int gridX,
            int gridY,
            int tileIndex,
            int rotation)
        {
            Tile tile = layout.TileSet.tiles[tileIndex];

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
                case 0:
                    return y * _tileResolution + x;

                case 1:
                    return (_tileResolution - 1 - x) * _tileResolution + y;

                case 2:
                    return (_tileResolution - 1 - y) * _tileResolution +
                           (_tileResolution - 1 - x);

                case 3:
                    return x * _tileResolution +
                           (_tileResolution - 1 - y);

                default:
                    return y * _tileResolution + x;
            }
        }

        #endregion
    }
}
