// TODO ROADMAP:
// [x] Normalize → pixel space conversion
// [ ] Cache tile pixels
// [ ] Use Color32 instead of Color
// [ ] Pre-rotate tiles
// [ ] Avoid per-frame allocations

using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    public class TextureRenderBackend
    {
        private Texture2D _output;

        public Texture2D Render(
            List<TileInstance> instances,
            TileSet[] tileSets,
            int gridWidth,
            int resolution)
        {
            if (_output == null || _output.width != resolution || _output.height != resolution)
            {
                _output = new Texture2D(
                    resolution,
                    resolution,
                    TextureFormat.RGBA32,
                    false);

                _output.filterMode = FilterMode.Point;
            }

            Color[] pixels = new Color[resolution * resolution];

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;

            foreach (var inst in instances)
            {
                DrawTile(pixels, resolution, inst, tileSets);
            }

            _output.SetPixels(pixels);
            _output.Apply();

            return _output;
        }

        private void DrawTile(
            Color[] target,
            int resolution,
            TileInstance inst,
            TileSet[] tileSets)
        {
            if (inst.TileSetId < 0 || inst.TileSetId >= tileSets.Length)
                return;

            var set = tileSets[inst.TileSetId];
            if (set == null || set.tiles == null)
                return;

            if (inst.TileIndex < 0 || inst.TileIndex >= set.tiles.Length)
                return;

            var tile = set.tiles[inst.TileIndex];
            if (tile == null || tile.texture == null)
                return;

            Texture2D tex = tile.texture;

            // NORMALIZED → PIXEL
            float sizePx = inst.Size * resolution;
            int size = Mathf.RoundToInt(sizePx);

            Vector2 center = inst.Position * resolution;

            int startX = Mathf.RoundToInt(center.x - sizePx * 0.5f);
            int startY = Mathf.RoundToInt(center.y - sizePx * 0.5f);

            Color[] src = tex.GetPixels();
            int srcSize = tex.width;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dstX = startX + x;
                    int dstY = startY + y;

                    if (dstX < 0 || dstY < 0 || dstX >= resolution || dstY >= resolution)
                        continue;

                    float u = (float)x / size;
                    float v = (float)y / size;

                    ApplyRotation(ref u, ref v, inst.Rotation);

                    int srcX = Mathf.Clamp((int)(u * srcSize), 0, srcSize - 1);
                    int srcY = Mathf.Clamp((int)(v * srcSize), 0, srcSize - 1);

                    Color color = src[srcY * srcSize + srcX];

                    target[dstY * resolution + dstX] = color;
                }
            }
        }

        private void ApplyRotation(ref float u, ref float v, int rotation)
        {
            switch (rotation & 3)
            {
                case 1:
                    (u, v) = (v, 1f - u);
                    break;
                case 2:
                    u = 1f - u;
                    v = 1f - v;
                    break;
                case 3:
                    (u, v) = (1f - v, u);
                    break;
            }
        }
    }
}