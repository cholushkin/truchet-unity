using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    // TODO ROADMAP:
    // [x] Integer raster pipeline (stable)
    // [x] Nearest sampling
    // [x] Coverage sampling (integer domain)
    // [x] Bilinear sampling (integer domain, UV-free)
    // [x] Optional pixel cache
    // [ ] Output buffer reuse
    // [ ] SIMD / Burst optimization
    // [ ] Chunk rendering

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
            Clear(pixels, _options.BackgroundColor);

            instances.Sort((a, b) => b.Size.CompareTo(a.Size));

            foreach (var inst in instances)
            {
                DrawTile(pixels, resolution, inst, tileSets);
            }

            if (_options.ApplyBinaryThreshold)
            {
                ApplyBinaryThreshold(pixels);
            }

            _output.SetPixels32(pixels);
            _output.Apply();

            return _output;
        }

        // --------------------------------------------------
        // Core Draw
        // --------------------------------------------------

        private void DrawTile(
            Color32[] target,
            int resolution,
            TileInstance inst,
            TileSet[] tileSets)
        {
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

                    Color32 color = Sample(
                        x, y, size,
                        srcPixels, tex, texW, texH,
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
        // Sampling Dispatcher
        // --------------------------------------------------

        private Color32 Sample(
            int x, int y, int size,
            Color32[] pixels,
            Texture2D tex,
            int texW, int texH,
            int rotation)
        {
            switch (_options.SamplingMode)
            {
                case TileSamplingMode.Nearest:
                    return SampleNearest(x, y, size, pixels, texW, texH, rotation);

                case TileSamplingMode.Coverage:
                    return SampleCoverage(x, y, size, pixels, texW, texH, rotation);

                case TileSamplingMode.Bilinear:
                    return SampleBilinear(x, y, size, pixels, texW, texH, rotation);

                default:
                    return default;
            }
        }

        // --------------------------------------------------
        // NEAREST (integer perfect)
        // --------------------------------------------------

        private Color32 SampleNearest(
            int x, int y, int size,
            Color32[] pixels,
            int texW, int texH,
            int rotation)
        {
            int srcX = (x * texW) / size;
            int srcY = (y * texH) / size;

            Clamp(ref srcX, ref srcY, texW, texH);
            ApplyRotationInt(ref srcX, ref srcY, texW, texH, rotation);

            return pixels[srcY * texW + srcX];
        }

        // --------------------------------------------------
        // COVERAGE (integer MSAA)
        // --------------------------------------------------

        private Color32 SampleCoverage(
            int x, int y, int size,
            Color32[] pixels,
            int texW, int texH,
            int rotation)
        {
            const int N = 4;

            int hits = 0;

            for (int sy = 0; sy < N; sy++)
            {
                for (int sx = 0; sx < N; sx++)
                {
                    // ✅ centered subpixel offsets
                    float ox = (sx + 0.5f) / N;
                    float oy = (sy + 0.5f) / N;

                    float fx = (x + ox) * texW / size;
                    float fy = (y + oy) * texH / size;

                    int srcX = (int)fx;
                    int srcY = (int)fy;

                    if (srcX >= texW) srcX = texW - 1;
                    if (srcY >= texH) srcY = texH - 1;

                    ApplyRotationInt(ref srcX, ref srcY, texW, texH, rotation);

                    if (pixels[srcY * texW + srcX].a > 127)
                        hits++;
                }
            }

            float coverage = hits / (float)(N * N);

            byte value = (byte)(coverage * 255f);

            return new Color32(value, value, value, value);
        }

        // --------------------------------------------------
        // BILINEAR (integer interpolation)
        // --------------------------------------------------

        private Color32 SampleBilinear(
            int x, int y, int size,
            Color32[] pixels,
            int texW, int texH,
            int rotation)
        {
            // --- normalized UV (pixel center)
            float u = (x + 0.5f) / size;
            float v = (y + 0.5f) / size;

            // --- rotate in UV space (CORRECT)
            ApplyRotationUV(ref u, ref v, rotation);

            // --- map to texture space
            float fx = u * texW - 0.5f;
            float fy = v * texH - 0.5f;

            int x0 = Mathf.FloorToInt(fx);
            int y0 = Mathf.FloorToInt(fy);

            int x1 = x0 + 1;
            int y1 = y0 + 1;

            float tx = fx - x0;
            float ty = fy - y0;

            // clamp
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

        // --------------------------------------------------
        // Rotation
        // --------------------------------------------------

        private void ApplyRotationInt(
            ref int x,
            ref int y,
            int width,
            int height,
            int rotation)
        {
            switch (rotation & 3)
            {
                case 1: (x, y) = (y, width - 1 - x); break;
                case 2: x = width - 1 - x; y = height - 1 - y; break;
                case 3: (x, y) = (height - 1 - y, x); break;
            }
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

        // --------------------------------------------------
        // Helpers
        // --------------------------------------------------

        private void Clamp(ref int x, ref int y, int w, int h)
        {
            if (x >= w) x = w - 1;
            if (y >= h) y = h - 1;
        }

        private void Blend(Color32[] target, int index, Color32 src)
        {
            if (src.a == 0) return;

            byte invA = (byte)(255 - src.a);
            Color32 dst = target[index];

            target[index] = new Color32(
                (byte)((src.r * src.a + dst.r * invA) / 255),
                (byte)((src.g * src.a + dst.g * invA) / 255),
                (byte)((src.b * src.a + dst.b * invA) / 255),
                255
            );
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

        private void EnsureOutput(int resolution)
        {
            if (_output == null || _output.width != resolution || _output.height != resolution)
            {
                _output = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
                _output.filterMode = FilterMode.Bilinear;
            }
        }

        private void Clear(Color32[] pixels, Color color)
        {
            Color32 c = color;
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = c;
        }
        
        private void ApplyBinaryThreshold(Color32[] pixels)
        {
            const int threshold = 127;

            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 c = pixels[i];

                // compute brightness (simple average)
                int brightness = (c.r + c.g + c.b) / 3;

                pixels[i] = brightness > threshold
                    ? new Color32(255, 255, 255, 255)
                    : new Color32(0, 0, 0, 255);
            }
        }
    }
}