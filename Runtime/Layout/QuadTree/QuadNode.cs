// TODO ROADMAP:
// [x] Quad node info struct
// [ ] Add transform helper
// [ ] Add world-space conversion helpers

namespace Truchet
{
    public struct QuadNode
    {
        public float X;
        public float Y;
        public float Size;
        public int Level;

        public bool IsLeaf;
        public bool IsActive;

        public int ChildIndex;
        public int ParentIndex;

        public int TileSetId;
        public int TileIndex;
        public int Rotation;
    }
}