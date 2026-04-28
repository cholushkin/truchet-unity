using System;

namespace Truchet
{
    public static class QuadTreeSnapshotExtensions
    {
        // ============================================================
        // CREATE
        // ============================================================

        public static QuadTreeStructureSnapshot CreateStructureSnapshot(this QuadTree quad)
        {
            if (quad == null)
                throw new Exception("QuadTree is null");

            int count = quad.NodeCount;

            var snapshot = new QuadTreeStructureSnapshot
            {
                RootSize = quad.RootSize,
                NodeCount = count,
                Nodes = new int[count]
            };

            for (int i = 0; i < count; i++)
            {
                snapshot.Nodes[i] = quad.Nodes[i].Pack();
            }

            return snapshot;
        }

        public static QuadTreeTileSnapshot CreateTileSnapshot(this QuadTree quad)
        {
            if (quad == null)
                throw new Exception("QuadTree is null");

            int count = quad.NodeCount;

            var snapshot = new QuadTreeTileSnapshot
            {
                NodeCount = count,
                TileIds = new byte[count]
            };

            Array.Copy(quad.TileIds, snapshot.TileIds, count);

            return snapshot;
        }

        // ============================================================
        // APPLY
        // ============================================================

        public static void ApplyStructureSnapshot(this QuadTree quad, QuadTreeStructureSnapshot snapshot)
        {
            if (quad == null)
                throw new Exception("QuadTree is null");

            if (snapshot.Nodes == null)
                throw new Exception("Snapshot nodes are null");

            // Rebuild nodes array
            quad.Clear();

            quad.SetRootSize(snapshot.RootSize);
            quad.Resize(snapshot.NodeCount);

            for (int i = 0; i < snapshot.NodeCount; i++)
            {
                quad.Nodes[i] = QuadNode.Unpack(snapshot.Nodes[i]);
            }
        }

        public static void ApplyTileSnapshot(this QuadTree quad, QuadTreeTileSnapshot snapshot)
        {
            if (quad == null)
                throw new Exception("QuadTree is null");

            if (snapshot.TileIds == null)
                throw new Exception("Tile snapshot is null");

            if (snapshot.NodeCount != quad.NodeCount)
                throw new Exception("Tile snapshot does not match QuadTree node count");

            Array.Copy(snapshot.TileIds, quad.TileIds, snapshot.NodeCount);
        }
    }
}