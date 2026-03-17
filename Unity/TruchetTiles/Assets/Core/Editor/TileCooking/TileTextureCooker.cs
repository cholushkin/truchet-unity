// TODO ROADMAP:
// [x] Unified logical→visual mapping
// [x] Winged 2x resolution support
// [x] Transparent outer margin for winged tiles
// [x] Explicit logical square fill
// [x] Base parity cooking (no baked inversion)
// [x] Centralized coordinate mapping
// [x] PNG saving
// [ ] Anti-aliasing (supersample or SDF)
// [ ] Parametric color support (non black/white)
// [ ] Texture atlas cooking
// [ ] GPU-based baking backend
// [ ] Editor preview window
// [ ] Batch cook multiple definitions
// [ ] Parity debug overlay mode

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Truchet
{
    public static class TileTextureCooker
    {
        // Wing geometry invariants (do NOT expose to inspector)
        private const float WingScale = 0.5f; // logical domain occupies center 50%
        private const float WingOffset = 0.25f; // 25% margin on each side

        // --------------------------------------------------
        // Public Entry
        // --------------------------------------------------

        public static Tile Cook(TileCookDefinition definition)
        {
            EnsureFolder(definition.OutputFolder);

            Texture2D texture = Rasterize(definition);

            string basePath = "Assets/" + definition.OutputFolder;

            string pngPath = Path.Combine(basePath, definition.name + ".png");
            string tilePath = Path.Combine(basePath, definition.name + "_Tile.asset");

            SavePNG(texture, pngPath);

            AssetDatabase.Refresh();

            Texture2D importedTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);

            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.connectivityMask = definition.Topology.Mask;
            tile.texture = importedTexture;
            tile.IsWinged = definition.IsWinged;

            AssetDatabase.CreateAsset(tile, tilePath);
            AssetDatabase.SaveAssets();

            return tile;
        }

        // --------------------------------------------------
        // Rasterization
        // --------------------------------------------------

        private static Texture2D Rasterize(TileCookDefinition def)
        {
            int baseWidth = def.Width;
            int baseHeight = def.Height;

            bool winged = def.IsWinged;

            int texWidth = winged ? baseWidth * 2 : baseWidth;
            int texHeight = winged ? baseHeight * 2 : baseHeight;

            float scale = winged ? WingScale : 1f;
            float offset = winged ? WingOffset : 0f;

            Texture2D texture =
                new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);

            Color[] pixels = new Color[texWidth * texHeight];

            // --------------------------------------------------
            // Clear background
            // --------------------------------------------------

            Color clear = winged
                ? new Color(0, 0, 0, 0) // transparent outside logical square
                : Color.white; // regular tiles fill entire domain

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = clear;

            // --------------------------------------------------
            // Base parity model
            // --------------------------------------------------
            // We ALWAYS cook base motif:
            // Logical square = white
            // Geometry drawn via instruction color
            // Runtime handles inversion per level parity

            if (winged)
            {
                FillLogicalSquare(pixels, texWidth, texHeight, Color.white);
            }

            // --------------------------------------------------
            // Execute render commands
            // --------------------------------------------------

            List<TileRenderInstruction> instructions =
                TileCommandParser.Parse(def.CommandScript);

            foreach (var instruction in instructions)
            {
                Color drawColor = instruction.Color == DrawColor.Black
                    ? Color.black
                    : Color.white;

                switch (instruction)
                {
                    case RectangleInstruction rectangle:
                        RasterizeRectangle(rectangle, pixels, texWidth, texHeight, scale, offset, drawColor);
                        break;

                    case EllipseInstruction ellipse:
                        RasterizeEllipse(ellipse, pixels, texWidth, texHeight, scale, offset, drawColor);
                        break;

                    case PieInstruction pie:
                        RasterizePie(pie, pixels, texWidth, texHeight, scale, offset, drawColor);
                        break;

                    case BezierInstruction bezier:
                        RasterizeBezier(bezier, pixels, texWidth, texHeight, scale, offset, drawColor);
                        break;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }
        // --------------------------------------------------
        // Logical → Pixel Mapping
        // --------------------------------------------------

        private static Vector2 MapLogical(
            Vector2 logical,
            int width,
            int height,
            float scale,
            float offset)
        {
            float x = (offset + logical.x * scale) * width;
            float y = (offset + logical.y * scale) * height;
            return new Vector2(x, y);
        }

        // --------------------------------------------------
        // Logical Square Fill
        // --------------------------------------------------

        private static void FillLogicalSquare(
            Color[] pixels,
            int width,
            int height,
            Color logicalColor)
        {
            int startX = Mathf.RoundToInt(width * WingOffset);
            int endX = Mathf.RoundToInt(width * (WingOffset + WingScale));

            int startY = Mathf.RoundToInt(height * WingOffset);
            int endY = Mathf.RoundToInt(height * (WingOffset + WingScale));

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    pixels[y * width + x] = logicalColor;
                }
            }
        }

        // --------------------------------------------------
        // Rasterizers
        // --------------------------------------------------

        private static void RasterizeRectangle(
            RectangleInstruction rect,
            Color[] pixels,
            int width,
            int height,
            float scale,
            float offset,
            Color motifColor)
        {
            Vector2 center = MapLogical(rect.Center, width, height, scale, offset);

            float sx = rect.Size.x * scale * width * 0.5f;
            float sy = rect.Size.y * scale * height * 0.5f;

            int cx = Mathf.RoundToInt(center.x);
            int cy = Mathf.RoundToInt(center.y);

            for (int y = (int)(cy - sy); y < (int)(cy + sy); y++)
            {
                if (y < 0 || y >= height) continue;

                for (int x = (int)(cx - sx); x < (int)(cx + sx); x++)
                {
                    if (x < 0 || x >= width) continue;

                    pixels[y * width + x] = motifColor;
                }
            }
        }

        private static void RasterizeEllipse(
            EllipseInstruction ellipse,
            Color[] pixels,
            int width,
            int height,
            float scale,
            float offset,
            Color motifColor)
        {
            Vector2 center = MapLogical(ellipse.Center, width, height, scale, offset);

            float rx = ellipse.Size.x * scale * width * 0.5f;
            float ry = ellipse.Size.y * scale * height * 0.5f;

            int cx = Mathf.RoundToInt(center.x);
            int cy = Mathf.RoundToInt(center.y);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - cx) / rx;
                    float dy = (y - cy) / ry;

                    if (dx * dx + dy * dy <= 1f)
                        pixels[y * width + x] = motifColor;
                }
            }
        }

        private static void RasterizePie(
            PieInstruction pie,
            Color[] pixels,
            int width,
            int height,
            float scale,
            float offset,
            Color motifColor)
        {
            Vector2 center = MapLogical(pie.Center, width, height, scale, offset);

            float rx = pie.Size.x * scale * width * 0.5f;
            float ry = pie.Size.y * scale * height * 0.5f;

            int cx = Mathf.RoundToInt(center.x);
            int cy = Mathf.RoundToInt(center.y);

            float start = pie.StartAngle;
            float end = start + pie.SweepAngle;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;

                    float nx = dx / rx;
                    float ny = dy / ry;

                    if (nx * nx + ny * ny > 1f)
                        continue;

                    float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    if (angle < 0)
                        angle += 360f;

                    if (IsAngleWithin(angle, start, end))
                        pixels[y * width + x] = motifColor;
                }
            }
        }

        private static bool IsAngleWithin(float angle, float start, float end)
        {
            if (end < start)
                end += 360f;

            if (angle < start)
                angle += 360f;

            return angle >= start && angle <= end;
        }

        private static void RasterizeBezier(
            BezierInstruction bezier,
            Color[] pixels,
            int width,
            int height,
            float scale,
            float offset,
            Color motifColor)
        {
            const int segments = 32;

            Vector2 prevLogical = bezier.P0;

            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector2 currLogical = EvaluateBezier(bezier, t);

                Vector2 a = MapLogical(prevLogical, width, height, scale, offset);
                Vector2 b = MapLogical(currLogical, width, height, scale, offset);

                float radius = bezier.Thickness * scale * width * 0.5f;

                DrawThickLine(a, b, radius, pixels, width, height, motifColor);

                prevLogical = currLogical;
            }
        }

        private static Vector2 EvaluateBezier(BezierInstruction b, float t)
        {
            float u = 1f - t;

            return
                u * u * u * b.P0 +
                3f * u * u * t * b.P1 +
                3f * u * t * t * b.P2 +
                t * t * t * b.P3;
        }

        private static void DrawThickLine(
            Vector2 a,
            Vector2 b,
            float radius,
            Color[] pixels,
            int width,
            int height,
            Color motifColor)
        {
            int steps = 16;

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 p = Vector2.Lerp(a, b, t);

                int cx = Mathf.RoundToInt(p.x);
                int cy = Mathf.RoundToInt(p.y);

                for (int y = (int)(cy - radius); y <= (int)(cy + radius); y++)
                {
                    if (y < 0 || y >= height) continue;

                    for (int x = (int)(cx - radius); x <= (int)(cx + radius); x++)
                    {
                        if (x < 0 || x >= width) continue;

                        float dx = x - cx;
                        float dy = y - cy;

                        if (dx * dx + dy * dy <= radius * radius)
                            pixels[y * width + x] = motifColor;
                    }
                }
            }
        }

        // --------------------------------------------------
        // IO Helpers
        // --------------------------------------------------

        private static void SavePNG(Texture2D texture, string path)
        {
            byte[] png = texture.EncodeToPNG();
            File.WriteAllBytes(path, png);
        }

        private static void EnsureFolder(string relativeFolder)
        {
            string fullPath = Path.Combine("Assets", relativeFolder);

            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                string[] parts = relativeFolder.Split('/');
                string current = "Assets";

                foreach (string part in parts)
                {
                    string next = current + "/" + part;

                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, part);

                    current = next;
                }
            }
        }
    }
}