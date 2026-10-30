// TODO ROADMAP:
// [x] Use internal QuadTree access for reconstruction
// [x] Remove fake rebuild logic
// [x] Restore allocator state
// [ ] Add validation for corrupted snapshots
// [ ] Add versioning support

using System;
using System.Collections.Generic;

namespace Truchet
{
    public static class QuadTreeSnapshotExtensions
    {
        // ============================================================
        // CREATE
        // ============================================================

        public static QuadTreeStructureSnapshot CreateStructureSnapshot(this QuadTree quad)
        {
            int count = quad.NodeCount;

            var nodes = new int[count * 8];

            for (int i = 0; i < count; i++)
            {
                var n = quad.GetNode(i);

                int o = i * 8;

                nodes[o + 0] = BitConverter.SingleToInt32Bits(n.X);
                nodes[o + 1] = BitConverter.SingleToInt32Bits(n.Y);
                nodes[o + 2] = BitConverter.SingleToInt32Bits(n.Size);
                nodes[o + 3] = n.Level;
                nodes[o + 4] = n.IsLeaf ? 1 : 0;
                nodes[o + 5] = n.ChildIndex;
                nodes[o + 6] = n.ParentIndex;
                nodes[o + 7] = n.IsActive ? 1 : 0;
            }

            return new QuadTreeStructureSnapshot
            {
                RootSize = 1f,
                NodeCount = count,
                Nodes = nodes
            };
        }

        public static QuadTreeTileSnapshot CreateTileSnapshot(this QuadTree quad)
        {
            int count = quad.NodeCount;

            var data = new int[count * 3];

            for (int i = 0; i < count; i++)
            {
                var n = quad.GetNode(i);

                int o = i * 3;

                data[o + 0] = n.TileSetId;
                data[o + 1] = n.TileIndex;
                data[o + 2] = n.Rotation;
            }

            return new QuadTreeTileSnapshot
            {
                NodeCount = count,
                TileIds = IntArrayToByteArray(data)
            };
        }

        // ============================================================
        // APPLY
        // ============================================================

        public static void ApplyStructureSnapshot(
            this QuadTree quad,
            QuadTreeStructureSnapshot snapshot)
        {
            int count = snapshot.NodeCount;
            var raw = snapshot.Nodes;

            quad.ClearInternal();

            var nodes = quad.Nodes;
            var free = quad.FreeBlocks;

            nodes.Capacity = count;

            for (int i = 0; i < count; i++)
            {
                int o = i * 8;

                nodes.Add(new QuadTree.QuadNode
                {
                    X = BitConverter.Int32BitsToSingle(raw[o + 0]),
                    Y = BitConverter.Int32BitsToSingle(raw[o + 1]),
                    Size = BitConverter.Int32BitsToSingle(raw[o + 2]),
                    Level = raw[o + 3],
                    IsLeaf = raw[o + 4] == 1,
                    ChildIndex = raw[o + 5],
                    ParentIndex = raw[o + 6],
                    IsActive = raw[o + 7] == 1
                });
            }

            // rebuild free blocks (aligned by 4)
            for (int i = 0; i < nodes.Count; i++)
            {
                if (!nodes[i].IsActive && i % 4 == 0)
                    free.Push(i);
            }
        }

        public static void ApplyTileSnapshot(
            this QuadTree quad,
            QuadTreeTileSnapshot snapshot)
        {
            int[] data = ByteArrayToIntArray(snapshot.TileIds);

            for (int i = 0; i < snapshot.NodeCount; i++)
            {
                int o = i * 3;

                quad.SetTileByNode(
                    i,
                    data[o + 0],
                    data[o + 1],
                    data[o + 2]);
            }
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private static byte[] IntArrayToByteArray(int[] data)
        {
            byte[] bytes = new byte[data.Length * 4];
            Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static int[] ByteArrayToIntArray(byte[] bytes)
        {
            int[] data = new int[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, data, 0, bytes.Length);
            return data;
        }
    }
}