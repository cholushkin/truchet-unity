// TODO ROADMAP:
// [x] Multi-TileSet Texture2DArray builder
// [x] Global motif index mapping
// [x] Hash-based caching
// [ ] Add partial rebuild support
// [ ] Add async GPU upload
// [ ] Add addressables support

using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    public class TileSetGPUResource
    {
        public Texture2DArray TextureArray;

        // TileSetId → start index in array
        public Dictionary<int, int> TileSetOffsets =
            new Dictionary<int, int>();
    }

    public static class TileArrayBuilder
    {
        private static TileSetGPUResource _cachedResource;
        private static int _cachedHash;

        public static TileSetGPUResource Build(TileSet[] tileSets)
        {
            if (tileSets == null || tileSets.Length == 0)
                return null;

            int hash = ComputeHash(tileSets);

            // ✅ cache hit
            if (_cachedResource != null && hash == _cachedHash)
                return _cachedResource;

            // 🔥 rebuild
            var resource = BuildInternal(tileSets);

            _cachedResource = resource;
            _cachedHash = hash;

            return resource;
        }

        // --------------------------------------------------
        // INTERNAL BUILD
        // --------------------------------------------------

        private static TileSetGPUResource BuildInternal(TileSet[] tileSets)
        {
            int totalTiles = 0;

            foreach (var set in tileSets)
            {
                if (set == null || set.tiles == null)
                    continue;

                totalTiles += set.tiles.Length;
            }

            if (totalTiles == 0)
                return null;

            Texture2D first = FindFirstTexture(tileSets);

            if (first == null)
            {
                Debug.LogError("No valid textures found in TileSets.");
                return null;
            }

            int width = first.width;
            int height = first.height;

            Texture2DArray array = new Texture2DArray(
                width,
                height,
                totalTiles,
                TextureFormat.RGBA32,
                false);

            var resource = new TileSetGPUResource();
            resource.TextureArray = array;

            int offset = 0;

            for (int setId = 0; setId < tileSets.Length; setId++)
            {
                var set = tileSets[setId];

                if (set == null || set.tiles == null)
                    continue;

                resource.TileSetOffsets[setId] = offset;

                for (int i = 0; i < set.tiles.Length; i++)
                {
                    var tile = set.tiles[i];

                    if (tile == null || tile.texture == null)
                    {
                        Debug.LogWarning($"Missing tile texture in TileSet {setId}");
                        continue;
                    }

                    var tex = tile.texture;

                    if (tex.width != width || tex.height != height)
                    {
                        Debug.LogError(
                            $"Texture mismatch in TileSet {setId}: " +
                            $"{tex.width}x{tex.height} vs {width}x{height}");
                        continue;
                    }

                    array.SetPixels(tex.GetPixels(), offset);
                    offset++;
                }
            }

            array.Apply();
            return resource;
        }

        // --------------------------------------------------
        // HASHING
        // --------------------------------------------------

        private static int ComputeHash(TileSet[] tileSets)
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 31 + tileSets.Length;

                for (int i = 0; i < tileSets.Length; i++)
                {
                    var set = tileSets[i];

                    if (set == null)
                    {
                        hash = hash * 31;
                        continue;
                    }

                    hash = hash * 31 + set.GetInstanceID();

                    if (set.tiles == null)
                    {
                        hash = hash * 31;
                        continue;
                    }

                    hash = hash * 31 + set.tiles.Length;

                    for (int t = 0; t < set.tiles.Length; t++)
                    {
                        var tile = set.tiles[t];

                        if (tile == null)
                        {
                            hash = hash * 31;
                            continue;
                        }

                        hash = hash * 31 + tile.GetInstanceID();

                        if (tile.texture != null)
                            hash = hash * 31 + tile.texture.GetInstanceID();
                    }
                }

                return hash;
            }
        }

        // --------------------------------------------------
        // HELPERS
        // --------------------------------------------------

        private static Texture2D FindFirstTexture(TileSet[] tileSets)
        {
            foreach (var set in tileSets)
            {
                if (set == null || set.tiles == null)
                    continue;

                foreach (var tile in set.tiles)
                {
                    if (tile != null && tile.texture != null)
                        return tile.texture;
                }
            }

            return null;
        }
    }
}