// TODO ROADMAP:
// [x] Parse command script
// [x] Polymorphic rasterization dispatch
// [x] Rectangle rasterization
// [ ] Ellipse rasterization
// [ ] Pie rasterization
// [ ] Bezier rasterization
// [ ] Anti-aliased rasterization
// [ ] GPU RenderTexture backend
// [ ] Atlas cooking
// [ ] Batch cooking

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Truchet.Tiles;

namespace Truchet.TileCooking
{
    public static class TileTextureCooker
    {
        public static Tile Cook(TileCookDefinition definition)
        {
            EnsureFolder(definition.OutputFolder);

            Texture2D texture = Rasterize(definition);

            string basePath = "Assets/" + definition.OutputFolder;

            string texturePath = Path.Combine(basePath, definition.name + "_Tex.asset");
            string tilePath = Path.Combine(basePath, definition.name + "_Tile.asset");

            AssetDatabase.CreateAsset(texture, texturePath);

            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.connectivityMask = definition.Topology.Mask;
            tile.texture = texture;

            AssetDatabase.CreateAsset(tile, tilePath);
            AssetDatabase.SaveAssets();

            return tile;
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

        private static Texture2D Rasterize(TileCookDefinition def)
        {
            int width = def.Width;
            int height = def.Height;

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            // Fill white background
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;

            List<TileRenderInstruction> instructions =
                TileCommandParser.Parse(def.CommandScript);

            foreach (TileRenderInstruction instruction in instructions)
            {
                switch (instruction)
                {
                    case RectangleInstruction rectangle:
                        RasterizeRectangle(rectangle, pixels, width, height);
                        break;

                    case EllipseInstruction ellipse:
                        RasterizeEllipse(ellipse, pixels, width, height);
                        break;

                    case PieInstruction pie:
                        RasterizePie(pie, pixels, width, height);
                        break;

                    case BezierInstruction bezier:
                        RasterizeBezier(bezier, pixels, width, height);
                        break;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        #region Rasterizers

        private static void RasterizeRectangle(
            RectangleInstruction rect,
            Color[] pixels,
            int width,
            int height)
        {
            int cx = Mathf.RoundToInt(rect.Center.x * width);
            int cy = Mathf.RoundToInt(rect.Center.y * height);

            int sx = Mathf.RoundToInt(rect.Size.x * width * 0.5f);
            int sy = Mathf.RoundToInt(rect.Size.y * height * 0.5f);

            for (int y = cy - sy; y < cy + sy; y++)
            {
                if (y < 0 || y >= height)
                    continue;

                for (int x = cx - sx; x < cx + sx; x++)
                {
                    if (x < 0 || x >= width)
                        continue;

                    pixels[y * width + x] = Color.black;
                }
            }
        }

        private static void RasterizeEllipse(
            EllipseInstruction ellipse,
            Color[] pixels,
            int width,
            int height)
        {
            int cx = Mathf.RoundToInt(ellipse.Center.x * width);
            int cy = Mathf.RoundToInt(ellipse.Center.y * height);

            float rx = ellipse.Size.x * width * 0.5f;
            float ry = ellipse.Size.y * height * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - cx) / rx;
                    float dy = (y - cy) / ry;

                    if (dx * dx + dy * dy <= 1f)
                        pixels[y * width + x] = Color.black;
                }
            }
        }

        private static void RasterizePie(
            PieInstruction pie,
            Color[] pixels,
            int width,
            int height)
        {
            int cx = Mathf.RoundToInt(pie.Center.x * width);
            int cy = Mathf.RoundToInt(pie.Center.y * height);

            float rx = pie.Size.x * width * 0.5f;
            float ry = pie.Size.y * height * 0.5f;

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
                        pixels[y * width + x] = Color.black;
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
            int height)
        {
            const int segments = 32;

            Vector2 prev = bezier.P0;

            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector2 point = EvaluateBezier(bezier, t);

                DrawThickLine(prev, point, bezier.Thickness, pixels, width, height);

                prev = point;
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
            float thickness,
            Color[] pixels,
            int width,
            int height)
        {
            int steps = 16;

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 p = Vector2.Lerp(a, b, t);

                int cx = Mathf.RoundToInt(p.x * width);
                int cy = Mathf.RoundToInt(p.y * height);

                int radius = Mathf.RoundToInt(thickness * width * 0.5f);

                for (int y = cy - radius; y <= cy + radius; y++)
                {
                    if (y < 0 || y >= height)
                        continue;

                    for (int x = cx - radius; x <= cx + radius; x++)
                    {
                        if (x < 0 || x >= width)
                            continue;

                        float dx = x - cx;
                        float dy = y - cy;

                        if (dx * dx + dy * dy <= radius * radius)
                            pixels[y * width + x] = Color.black;
                    }
                }
            }
        }

        #endregion
    }
}