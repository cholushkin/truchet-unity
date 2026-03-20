// TODO ROADMAP:
// [x] Logical tile instance (composition output)
// [ ] Add bounds metadata
// [ ] Add flags (visibility, lod, etc.)
// [ ] Add custom per-instance data (color, params)
// [ ] Add support for non-tile primitives

using UnityEngine;

namespace Truchet
{
    /// <summary>
    /// Renderer-agnostic tile instance.
    /// Produced by Composition layer.
    /// </summary>
    public struct TileInstance
    {
        public Vector2 Position;
        public float Size;

        public int TileSetId;
        public int TileIndex;
        public int Rotation;

        public int Level; // for QuadTree / LOD
    }
}