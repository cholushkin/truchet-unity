// TODO ROADMAP:
// [x] Introduce hierarchical tile node model
// [x] Separate leaf and container nodes
// [ ] Add bounds struct (RectInt)
// [ ] Add world-space transform data
// [ ] Add metadata payload support
// [ ] Add traversal utilities

namespace Truchet.Tiles
{
    public abstract class TileNode
    {
        public int X { get; }
        public int Y { get; }
        public int Level { get; }

        protected TileNode(int x, int y, int level)
        {
            X = x;
            Y = y;
            Level = level;
        }

        public abstract bool IsLeaf { get; }
    }

    public sealed class LeafTileNode : TileNode
    {
        public TileType TileType { get; }

        public LeafTileNode(int x, int y, int level, TileType tileType)
            : base(x, y, level)
        {
            TileType = tileType;
        }

        public override bool IsLeaf => true;
    }

    public sealed class ContainerTileNode : TileNode
    {
        // NW, NE, SE, SW
        public TileNode[] Children { get; }

        public ContainerTileNode(int x, int y, int level, TileNode[] children)
            : base(x, y, level)
        {
            if (children == null || children.Length != 4)
                throw new System.ArgumentException("Container must have 4 children.");

            Children = children;
        }

        public override bool IsLeaf => false;
    }
}