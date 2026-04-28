using System;
using System.Collections.Generic;

// ======================================================
// PACKED TILE (16-bit)
// ======================================================

public struct PackedTile
{
    public ushort Data;

    public static PackedTile Encode(int setId, int tileIndex, int rot)
    {
        ushort data = 0;

        int safeSet = setId < 0 ? 31 : setId;
        int safeTile = tileIndex < 0 ? 63 : tileIndex;

        data |= (ushort)(safeSet & 0x1F);
        data |= (ushort)((safeTile & 0x3F) << 5);
        data |= (ushort)((rot & 0x3) << 11);

        return new PackedTile { Data = data };
    }

    public void Decode(out int setId, out int tileIndex, out int rot)
    {
        setId = (Data & 0x1F);
        tileIndex = (Data >> 5) & 0x3F;
        rot = (Data >> 11) & 0x3;

        if (setId == 31) setId = -1;
        if (tileIndex == 63) tileIndex = -1;
    }
}

// ======================================================
// QUADTREE SNAPSHOT (FULL)
// ======================================================

public struct QuadNodeSnapshot
{
    public ushort Data;
}

public struct QuadTreeSnapshot
{
    public QuadNodeSnapshot[] Nodes;
    public int LogicalWidth;
    public int LogicalHeight;
    public uint Seed;
}

// ======================================================
// QUADTREE STRUCTURE ONLY
// ======================================================

public struct QuadTreeStructureSnapshot
{
    public byte[] Data;
    public int BitCount;
}

// ======================================================
// BIT WRITER
// ======================================================

internal class BitWriter
{
    private byte[] _data;
    private int _bitIndex;

    public BitWriter(int capacity = 128)
    {
        _data = new byte[capacity];
    }

    public void Write(bool bit)
    {
        int byteIndex = _bitIndex >> 3;
        int bitOffset = _bitIndex & 7;

        if (byteIndex >= _data.Length)
            Array.Resize(ref _data, _data.Length * 2);

        if (bit)
            _data[byteIndex] |= (byte)(1 << bitOffset);

        _bitIndex++;
    }

    public QuadTreeStructureSnapshot ToSnapshot()
    {
        int byteCount = (_bitIndex + 7) >> 3;

        var result = new byte[byteCount];
        Array.Copy(_data, result, byteCount);

        return new QuadTreeStructureSnapshot
        {
            Data = result,
            BitCount = _bitIndex
        };
    }
}

// ======================================================
// BIT READER
// ======================================================

internal struct BitReader
{
    private readonly byte[] _data;
    private readonly int _bitCount;
    private int _bitIndex;

    public BitReader(byte[] data, int bitCount)
    {
        _data = data;
        _bitCount = bitCount;
        _bitIndex = 0;
    }

    public bool Read()
    {
        if (_bitIndex >= _bitCount)
            throw new Exception("BitReader overflow");

        int byteIndex = _bitIndex >> 3;
        int bitOffset = _bitIndex & 7;

        bool value = (_data[byteIndex] & (1 << bitOffset)) != 0;

        _bitIndex++;
        return value;
    }
}

// ======================================================
// QUADTREE NODE PACKING
// ======================================================

public static class QuadNodeCodec
{
    public static QuadNodeSnapshot Encode(bool hasChildren, PackedTile tile)
    {
        ushort data = 0;

        if (hasChildren)
            data |= 1;

        data |= (ushort)(tile.Data << 1);

        return new QuadNodeSnapshot { Data = data };
    }

    public static void Decode(QuadNodeSnapshot node, out bool hasChildren, out PackedTile tile)
    {
        hasChildren = (node.Data & 1) != 0;

        tile = new PackedTile
        {
            Data = (ushort)(node.Data >> 1)
        };
    }
}

// ======================================================
// QUADTREE SERIALIZER (DFS)
// ======================================================

public static class QuadTreeSnapshotSerializer
{
    // Requires adapter functions to avoid coupling

    public static QuadTreeSnapshot Capture(
        int rootNode,
        Func<int, bool> hasChildren,
        Func<int, int> firstChild,
        Func<int, (int setId, int tileIndex, int rot)> getTile)
    {
        var nodes = new List<QuadNodeSnapshot>();

        void Traverse(int nodeIndex)
        {
            bool split = hasChildren(nodeIndex);

            var t = getTile(nodeIndex);
            var packed = PackedTile.Encode(t.setId, t.tileIndex, t.rot);

            nodes.Add(QuadNodeCodec.Encode(split, packed));

            if (split)
            {
                int c = firstChild(nodeIndex);

                Traverse(c + 0);
                Traverse(c + 1);
                Traverse(c + 2);
                Traverse(c + 3);
            }
        }

        Traverse(rootNode);

        return new QuadTreeSnapshot
        {
            Nodes = nodes.ToArray()
        };
    }

    public static void Apply(
        QuadTreeSnapshot snapshot,
        Action reset,
        Func<int> createRoot,
        Action<int> subdivide,
        Func<int, int> firstChild,
        Action<int, int, int, int> setTile)
    {
        int index = 0;

        reset();

        int root = createRoot();

        void Traverse(int nodeIndex)
        {
            var node = snapshot.Nodes[index++];

            QuadNodeCodec.Decode(node, out bool split, out PackedTile tile);

            tile.Decode(out int setId, out int tileIndex, out int rot);
            setTile(nodeIndex, setId, tileIndex, rot);

            if (split)
            {
                subdivide(nodeIndex);

                int c = firstChild(nodeIndex);

                Traverse(c + 0);
                Traverse(c + 1);
                Traverse(c + 2);
                Traverse(c + 3);
            }
        }

        Traverse(root);
    }
}

// ======================================================
// STRUCTURE SERIALIZER
// ======================================================

public static class QuadTreeStructureSerializer
{
    public static QuadTreeStructureSnapshot Capture(
        int rootNode,
        Func<int, bool> hasChildren,
        Func<int, int> firstChild)
    {
        var writer = new BitWriter();

        void Traverse(int nodeIndex)
        {
            bool split = hasChildren(nodeIndex);

            writer.Write(split);

            if (split)
            {
                int c = firstChild(nodeIndex);

                Traverse(c + 0);
                Traverse(c + 1);
                Traverse(c + 2);
                Traverse(c + 3);
            }
        }

        Traverse(rootNode);

        return writer.ToSnapshot();
    }

    public static void Apply(
        QuadTreeStructureSnapshot snapshot,
        Action reset,
        Func<int> createRoot,
        Action<int> subdivide,
        Func<int, int> firstChild)
    {
        var reader = new BitReader(snapshot.Data, snapshot.BitCount);

        reset();

        int root = createRoot();

        void Traverse(int nodeIndex)
        {
            bool split = reader.Read();

            if (split)
            {
                subdivide(nodeIndex);

                int c = firstChild(nodeIndex);

                Traverse(c + 0);
                Traverse(c + 1);
                Traverse(c + 2);
                Traverse(c + 3);
            }
        }

        Traverse(root);
    }
}