using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    // TODO ROADMAP:
    // [x] Pixel cache per texture
    // [ ] Add invalidation (texture versioning)
    // [ ] Add rotation cache
    // [ ] Add atlas support

    public static class TilePixelCache
    {
        private class CachedTile
        {
            public Color32[] pixels;
            public int width;
            public int height;
        }

        private static readonly Dictionary<Texture2D, CachedTile> _cache =
            new Dictionary<Texture2D, CachedTile>();

        public static bool TryGet(
            Texture2D texture,
            out Color32[] pixels,
            out int width,
            out int height)
        {
            if (texture == null)
            {
                pixels = null;
                width = height = 0;
                return false;
            }

            if (!_cache.TryGetValue(texture, out var cached))
            {
                cached = new CachedTile
                {
                    pixels = texture.GetPixels32(),
                    width = texture.width,
                    height = texture.height
                };

                _cache[texture] = cached;
            }

            pixels = cached.pixels;
            width = cached.width;
            height = cached.height;
            return true;
        }

        public static void Clear()
        {
            _cache.Clear();
        }
    }
}