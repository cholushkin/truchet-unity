using System;

namespace Truchet
{
    // ============================================================
    // STRUCTURE SNAPSHOT (tree topology only)
    // ============================================================

    [Serializable]
    public struct QuadTreeStructureSnapshot
    {
        public float RootSize;
        public int NodeCount;
        public int[] Nodes;
    }

    // ============================================================
    // TILE SNAPSHOT (tile data only)
    // ============================================================

    [Serializable]
    public struct QuadTreeTileSnapshot
    {
        public int NodeCount;
        public byte[] TileIds;
    }

    // ============================================================
    // FULL SNAPSHOT (combined)
    // ============================================================

    [Serializable]
    public struct QuadTreeFullSnapshot
    {
        public QuadTreeStructureSnapshot Structure;
        public QuadTreeTileSnapshot Tiles;
    }
}