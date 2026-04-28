using System;

namespace Truchet
{
    public static class SnapshotsController
    {
        // ------------------------------------------------------------
        // FULL SNAPSHOT
        // ------------------------------------------------------------

        public static byte[] Serialize(QuadTreeFullSnapshot snapshot)
        {
            Validate(snapshot);

            byte[] structureBytes = SerializeStructure(snapshot.Structure);
            byte[] tileBytes = SerializeTiles(snapshot.Tiles);

            // [structureSize:4][structureBytes][tileBytes]
            int totalSize = 4 + structureBytes.Length + tileBytes.Length;

            byte[] bytes = new byte[totalSize];

            int offset = 0;

            Buffer.BlockCopy(BitConverter.GetBytes(structureBytes.Length), 0, bytes, offset, 4);
            offset += 4;

            Buffer.BlockCopy(structureBytes, 0, bytes, offset, structureBytes.Length);
            offset += structureBytes.Length;

            Buffer.BlockCopy(tileBytes, 0, bytes, offset, tileBytes.Length);

            return bytes;
        }

        public static QuadTreeFullSnapshot Deserialize(byte[] bytes)
        {
            QuadTreeFullSnapshot snapshot = new QuadTreeFullSnapshot();

            int offset = 0;

            int structureSize = BitConverter.ToInt32(bytes, offset);
            offset += 4;

            byte[] structureBytes = new byte[structureSize];
            Buffer.BlockCopy(bytes, offset, structureBytes, 0, structureSize);
            offset += structureSize;

            int tileBytesLength = bytes.Length - offset;
            byte[] tileBytes = new byte[tileBytesLength];
            Buffer.BlockCopy(bytes, offset, tileBytes, 0, tileBytesLength);

            snapshot.Structure = DeserializeStructure(structureBytes);
            snapshot.Tiles = DeserializeTiles(tileBytes);

            Validate(snapshot);

            return snapshot;
        }

        // ------------------------------------------------------------
        // STRUCTURE
        // ------------------------------------------------------------

        public static byte[] SerializeStructure(QuadTreeStructureSnapshot snapshot)
        {
            int count = snapshot.NodeCount;

            int totalSize = 8 + count * 4;

            byte[] bytes = new byte[totalSize];

            int offset = 0;

            Buffer.BlockCopy(BitConverter.GetBytes(snapshot.RootSize), 0, bytes, offset, 4);
            offset += 4;

            Buffer.BlockCopy(BitConverter.GetBytes(snapshot.NodeCount), 0, bytes, offset, 4);
            offset += 4;

            Buffer.BlockCopy(snapshot.Nodes, 0, bytes, offset, count * 4);

            return bytes;
        }

        public static QuadTreeStructureSnapshot DeserializeStructure(byte[] bytes)
        {
            QuadTreeStructureSnapshot snapshot = new QuadTreeStructureSnapshot();

            int offset = 0;

            snapshot.RootSize = BitConverter.ToSingle(bytes, offset);
            offset += 4;

            snapshot.NodeCount = BitConverter.ToInt32(bytes, offset);
            offset += 4;

            int count = snapshot.NodeCount;

            snapshot.Nodes = new int[count];
            Buffer.BlockCopy(bytes, offset, snapshot.Nodes, 0, count * 4);

            return snapshot;
        }

        // ------------------------------------------------------------
        // TILE DATA
        // ------------------------------------------------------------

        public static byte[] SerializeTiles(QuadTreeTileSnapshot snapshot)
        {
            int totalSize = 4 + snapshot.NodeCount;

            byte[] bytes = new byte[totalSize];

            int offset = 0;

            Buffer.BlockCopy(BitConverter.GetBytes(snapshot.NodeCount), 0, bytes, offset, 4);
            offset += 4;

            Buffer.BlockCopy(snapshot.TileIds, 0, bytes, offset, snapshot.NodeCount);

            return bytes;
        }

        public static QuadTreeTileSnapshot DeserializeTiles(byte[] bytes)
        {
            QuadTreeTileSnapshot snapshot = new QuadTreeTileSnapshot();

            int offset = 0;

            snapshot.NodeCount = BitConverter.ToInt32(bytes, offset);
            offset += 4;

            snapshot.TileIds = new byte[snapshot.NodeCount];
            Buffer.BlockCopy(bytes, offset, snapshot.TileIds, 0, snapshot.NodeCount);

            return snapshot;
        }

        // ------------------------------------------------------------
        // VALIDATION
        // ------------------------------------------------------------

        private static void Validate(QuadTreeFullSnapshot snapshot)
        {
            if (snapshot.Structure.NodeCount != snapshot.Tiles.NodeCount)
                throw new Exception("Snapshot mismatch: NodeCount differs");

            if (snapshot.Structure.Nodes == null)
                throw new Exception("Structure nodes are null");

            if (snapshot.Tiles.TileIds == null)
                throw new Exception("Tile data is null");
        }
    }
}