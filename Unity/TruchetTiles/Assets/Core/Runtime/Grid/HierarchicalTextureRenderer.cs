// TODO ROADMAP:
// [x] Basic hierarchical CPU renderer
// [x] Leaf-only rendering
// [x] Large-to-small ordering
// [ ] Add parity inversion
// [ ] Add transform matrix abstraction
// [ ] Add multiscale debug overlay
// [ ] GPU backend replacement

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Truchet
{
    public class HierarchicalTextureRenderer
    {
        private readonly int _baseResolution;

        private readonly bool _debugStep;
        private readonly int _debugDelayMs;
        private readonly bool _waitForKey;

        public HierarchicalTextureRenderer(
            int baseResolution,
            bool debugStep = false,
            int debugDelayMs = 0,
            bool waitForKey = false)
        {
            _baseResolution = baseResolution;
            _debugStep = debugStep;
            _debugDelayMs = debugDelayMs;
            _waitForKey = waitForKey;
        }

        public async UniTask RenderAsync(
            IHierarchicalTileLayout layout,
            TileSet[] tileSets,
            Texture2D output,
            CancellationToken token = default)
        {
            int resolution = _baseResolution;

            Color[] pixels = new Color[resolution * resolution];

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;

            List<QuadNodeInfo> leaves = new List<QuadNodeInfo>();

            foreach (int index in layout.GetLeafIndices())
            {
                QuadNodeInfo node = layout.GetNode(index);
                if (node.IsActive)
                    leaves.Add(node);
            }

            leaves.Sort((a, b) => b.Size.CompareTo(a.Size));

            if (_debugStep)
            {
                Debug.Log("===== Hierarchical Render Start =====");
                Debug.Log($"Leaf Count: {leaves.Count}");
                Debug.Log($"Resolution: {resolution}");
            }

            int renderIndex = 0;

            foreach (var node in leaves)
            {
                token.ThrowIfCancellationRequested();

                if (node.TileSetId < 0 ||
                    node.TileSetId >= tileSets.Length)
                    continue;

                TileSet tileSet = tileSets[node.TileSetId];

                if (node.TileIndex < 0 ||
                    node.TileIndex >= tileSet.tiles.Length)
                    continue;

                Tile tile = tileSet.tiles[node.TileIndex];
                if (tile == null || tile.texture == null)
                    continue;

                if (_debugStep)
                {
                    Debug.Log(
                        $"--- Rendering Tile {renderIndex + 1}/{leaves.Count} ---\n" +
                        $"Level: {node.Level}\n" +
                        $"Size: {node.Size:F3}\n" +
                        $"Position: ({node.X:F3}, {node.Y:F3})\n" +
                        $"Rotation: {node.Rotation}\n" +
                        $"Parity Inverted: {(node.Level % 2 == 1)}"
                    );
                }

                BlitNode(tile, node, pixels, resolution);
                if (_debugStep)
                {
                    DrawNodeFrame(node, pixels, resolution);
                }

                output.SetPixels(pixels);
                output.Apply();

                renderIndex++;

                if (_debugStep)
                {
                    if (_waitForKey)
                    {
                        Debug.Log("Waiting for SPACE to continue...");
                        await UniTask.WaitUntil(
                            () => Keyboard.current.spaceKey.wasPressedThisFrame,
                            cancellationToken: token);
                    }
                    else if (_debugDelayMs > 0)
                    {
                        Debug.Log($"Waiting {_debugDelayMs} ms...");
                        await UniTask.Delay(_debugDelayMs, cancellationToken: token);
                    }
                    else
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                    }
                }
            }

            if (_debugStep)
                Debug.Log("===== Hierarchical Render Finished =====");
        }

        private void BlitNode(
            Tile tile,
            QuadNodeInfo node,
            Color[] target,
            int resolution)
        {
            Texture2D sourceTex = tile.texture;
            Color[] source = sourceTex.GetPixels();

            int sourceRes = sourceTex.width;

            int nodeSizePx = Mathf.RoundToInt(node.Size * resolution);

            // Center of node
            float centerX = (node.X + node.Size * 0.5f) * resolution;
            float centerY = (node.Y + node.Size * 0.5f) * resolution;

            int renderSize = nodeSizePx * 2; // winged tiles are 2x logical

            int startX = Mathf.RoundToInt(centerX - renderSize * 0.5f);
            int startY = Mathf.RoundToInt(centerY - renderSize * 0.5f);

            // Parity rule
            bool invert = (node.Level % 2 == 1);

            for (int y = 0; y < renderSize; y++)
            {
                for (int x = 0; x < renderSize; x++)
                {
                    int tx = startX + x;
                    int ty = startY + y;

                    if (tx < 0 || tx >= resolution ||
                        ty < 0 || ty >= resolution)
                        continue;

                    int srcX = Mathf.FloorToInt((float)x / renderSize * sourceRes);
                    int srcY = Mathf.FloorToInt((float)y / renderSize * sourceRes);

                    int srcIndex = srcY * sourceRes + srcX;
                    Color c = source[srcIndex];

                    if (c.a <= 0f)
                        continue;

                    if (invert)
                    {
                        // Swap black/white
                        if (c.r > 0.5f) // white
                            c = Color.black;
                        else // black
                            c = Color.white;
                    }

                    target[ty * resolution + tx] = c;
                }
            }
        }
        
        
        
        private void DrawNodeFrame(
            QuadNodeInfo node,
            Color[] pixels,
            int resolution)
        {
            bool InBounds(int x, int y, int resolution)
            {
                return x >= 0 && x < resolution &&
                       y >= 0 && y < resolution;
            }
            int startX = Mathf.RoundToInt(node.X * resolution);
            int startY = Mathf.RoundToInt(node.Y * resolution);
            int sizePx = Mathf.RoundToInt(node.Size * resolution);

            int endX = startX + sizePx - 1;
            int endY = startY + sizePx - 1;

            Color frameColor = Color.green;

            // Top & Bottom
            for (int x = startX; x <= endX; x++)
            {
                if (InBounds(x, startY, resolution))
                    pixels[startY * resolution + x] = frameColor;

                if (InBounds(x, endY, resolution))
                    pixels[endY * resolution + x] = frameColor;
            }

            // Left & Right
            for (int y = startY; y <= endY; y++)
            {
                if (InBounds(startX, y, resolution))
                    pixels[y * resolution + startX] = frameColor;

                if (InBounds(endX, y, resolution))
                    pixels[y * resolution + endX] = frameColor;
            }
        }
    }
    
}