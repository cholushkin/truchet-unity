using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    public class TextureRenderBackend
    {
        private Texture2D _output;
        private TileRenderOptions _options;

        public void SetOptions(TileRenderOptions options)
        {
            _options = options;
        }

        public Texture2D Render(
            List<TileInstance> instances,
            TileSet[] tileSets,
            int resolution)
        {
            EnsureOutput(resolution);

            Color32[] pixels = new Color32[resolution * resolution];

            Clear(pixels);

            // draw large → small
            instances.Sort((a, b) => b.Size.CompareTo(a.Size));

            foreach (var inst in instances)
            {
                DrawTile(pixels, resolution, inst, tileSets);
            }

            _output.SetPixels32(pixels);
            _output.Apply();

            return _output;
        }

        // --------------------------------------------------
        // DRAW TILE
        // --------------------------------------------------

        private void DrawTile(
            Color32[] target,
            int resolution,
            TileInstance inst,
            TileSet[] tileSets)
        {
            if (inst.TileSetId < 0 || inst.TileIndex < 0)
                return;

            if (inst.TileSetId >= tileSets.Length)
                return;

            var tileSet = tileSets[inst.TileSetId];
            if (tileSet == null || tileSet.tiles == null)
                return;

            if (inst.TileIndex >= tileSet.tiles.Length)
                return;
            
            
            if (!TryGetTile(inst, tileSets, out var tex, out var srcPixels, out int texW, out int texH))
                return;

            float logicalSizePx = inst.Size * resolution;
            float renderSizePx = inst.IsWinged ? logicalSizePx * 2f : logicalSizePx;

            int size = Mathf.RoundToInt(renderSizePx);

            Vector2 center = inst.Position * resolution;

            int startX = Mathf.RoundToInt(center.x - renderSizePx * 0.5f);
            int startY = Mathf.RoundToInt(center.y - renderSizePx * 0.5f);

            bool invert = (inst.Level & 1) == 1;

            for (int y = 0; y < size; y++)
            {
                int dstY = startY + y;
                if (dstY < 0 || dstY >= resolution) continue;

                for (int x = 0; x < size; x++)
                {
                    int dstX = startX + x;
                    if (dstX < 0 || dstX >= resolution) continue;

                    Color32 color = SampleBilinear(
                        x, y, size,
                        srcPixels, texW, texH,
                        inst.Rotation
                    );

                    if (invert)
                    {
                        color.r = (byte)(255 - color.r);
                        color.g = (byte)(255 - color.g);
                        color.b = (byte)(255 - color.b);
                    }

                    Blend(target, dstY * resolution + dstX, color);
                }
            }
        }

        // --------------------------------------------------
        // BILINEAR SAMPLING
        // --------------------------------------------------

        private Color32 SampleBilinear(
            int x, int y, int size,
            Color32[] pixels,
            int texW, int texH,
            int rotation)
        {
            float u = (x + 0.5f) / size;
            float v = (y + 0.5f) / size;

            ApplyRotationUV(ref u, ref v, rotation);

            float fx = u * texW - 0.5f;
            float fy = v * texH - 0.5f;

            int x0 = Mathf.FloorToInt(fx);
            int y0 = Mathf.FloorToInt(fy);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            float tx = fx - x0;
            float ty = fy - y0;

            x0 = Mathf.Clamp(x0, 0, texW - 1);
            y0 = Mathf.Clamp(y0, 0, texH - 1);
            x1 = Mathf.Clamp(x1, 0, texW - 1);
            y1 = Mathf.Clamp(y1, 0, texH - 1);

            Color32 c00 = pixels[y0 * texW + x0];
            Color32 c10 = pixels[y0 * texW + x1];
            Color32 c01 = pixels[y1 * texW + x0];
            Color32 c11 = pixels[y1 * texW + x1];

            return Lerp4(c00, c10, c01, c11, tx, ty);
        }

        private Color32 Lerp4(Color32 c00, Color32 c10, Color32 c01, Color32 c11, float tx, float ty)
        {
            float r =
                c00.r * (1 - tx) * (1 - ty) +
                c10.r * tx * (1 - ty) +
                c01.r * (1 - tx) * ty +
                c11.r * tx * ty;

            float g =
                c00.g * (1 - tx) * (1 - ty) +
                c10.g * tx * (1 - ty) +
                c01.g * (1 - tx) * ty +
                c11.g * tx * ty;

            float b =
                c00.b * (1 - tx) * (1 - ty) +
                c10.b * tx * (1 - ty) +
                c01.b * (1 - tx) * ty +
                c11.b * tx * ty;

            float a =
                c00.a * (1 - tx) * (1 - ty) +
                c10.a * tx * (1 - ty) +
                c01.a * (1 - tx) * ty +
                c11.a * tx * ty;

            return new Color32((byte)r, (byte)g, (byte)b, (byte)a);
        }

        private void ApplyRotationUV(ref float u, ref float v, int rotation)
        {
            switch (rotation & 3)
            {
                case 1: (u, v) = (v, 1f - u); break;
                case 2: u = 1f - u; v = 1f - v; break;
                case 3: (u, v) = (1f - v, u); break;
            }
        }

        private void Blend(Color32[] target, int index, Color32 src)
        {
            if (src.a == 0) return;

            Color32 dst = target[index];

            float srcA = src.a / 255f;
            float dstA = dst.a / 255f;

            float invA = 1f - srcA;

            float outA = srcA + dstA * invA;

            // convert to linear space
            float sr = Mathf.GammaToLinearSpace(src.r / 255f);
            float sg = Mathf.GammaToLinearSpace(src.g / 255f);
            float sb = Mathf.GammaToLinearSpace(src.b / 255f);

            float dr = Mathf.GammaToLinearSpace(dst.r / 255f);
            float dg = Mathf.GammaToLinearSpace(dst.g / 255f);
            float db = Mathf.GammaToLinearSpace(dst.b / 255f);

            // correct alpha-aware blending (this fixes the fringe)
            float r = sr * srcA + dr * dstA * invA;
            float g = sg * srcA + dg * dstA * invA;
            float b = sb * srcA + db * dstA * invA;

            if (outA > 0f)
            {
                r /= outA;
                g /= outA;
                b /= outA;
            }

            // back to gamma space
            r = Mathf.LinearToGammaSpace(r);
            g = Mathf.LinearToGammaSpace(g);
            b = Mathf.LinearToGammaSpace(b);

            target[index] = new Color32(
                (byte)(Mathf.Clamp01(r) * 255f),
                (byte)(Mathf.Clamp01(g) * 255f),
                (byte)(Mathf.Clamp01(b) * 255f),
                (byte)(outA * 255f)
            );
        }

        // --------------------------------------------------
        // HELPERS
        // --------------------------------------------------

        private void Clear(Color32[] pixels)
        {
            Color32 bg = (Color32)_options.BackgroundColor;

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = bg;
        }

        private void EnsureOutput(int resolution)
        {
            if (_output == null || _output.width != resolution || _output.height != resolution)
            {
                _output = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
                _output.filterMode = FilterMode.Bilinear;
            }
        }

        private bool TryGetTile(
            TileInstance inst,
            TileSet[] tileSets,
            out Texture2D tex,
            out Color32[] pixels,
            out int width,
            out int height)
        {
            tex = null;
            pixels = null;
            width = height = 0;

            if (inst.TileSetId < 0 || inst.TileSetId >= tileSets.Length)
                return false;

            var set = tileSets[inst.TileSetId];
            if (set?.tiles == null) return false;

            if (inst.TileIndex < 0 || inst.TileIndex >= set.tiles.Length)
                return false;

            var tile = set.tiles[inst.TileIndex];
            if (tile?.texture == null) return false;

            tex = tile.texture;

            if (_options.UseTilePixelCache)
                return TilePixelCache.TryGet(tex, out pixels, out width, out height);

            pixels = tex.GetPixels32();
            width = tex.width;
            height = tex.height;
            return true;
        }
    }
}