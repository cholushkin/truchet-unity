using Truchet;
using UnityEngine;

public class TilemapStateController : MonoBehaviour
{
    // =========================
    // TYPE
    // =========================

    private enum StateType : byte
    {
        None = 0,
        Grid = 1,
        QuadTree = 2
    }

    [SerializeField] private StateType _type;

    // =========================
    // STORED DATA
    // =========================

    [SerializeField] private byte[] _fullData;
    [SerializeField] private byte[] _structureData;

    private GridSnapshot _grid;
    private QuadTreeSnapshot _quad;
    private QuadTreeStructureSnapshot _structure;

    // =========================
    // CAPTURE FULL
    // =========================

    public void Capture(TruchetRuntime runtime)
    {
        if (runtime.IsGrid)
        {
            _type = StateType.Grid;

            _grid = runtime.CaptureGrid();
            _fullData = GridSnapshotSerializer.Serialize(_grid);
        }
        else
        {
            _type = StateType.QuadTree;

            _quad = runtime.CaptureSnapshot();
            _fullData = SerializeQuad(_quad);
        }
    }

    // =========================
    // APPLY FULL
    // =========================

    public void Apply(TruchetRuntime runtime)
    {
        if (_type == StateType.Grid)
        {
            if (_grid.Tiles == null || _grid.Tiles.Length == 0)
                _grid = GridSnapshotSerializer.Deserialize(_fullData);

            runtime.ApplyGrid(_grid);
        }
        else if (_type == StateType.QuadTree)
        {
            if (_quad.Nodes == null || _quad.Nodes.Length == 0)
                _quad = DeserializeQuad(_fullData);

            runtime.ApplySnapshot(_quad);
        }

        runtime.RebuildComposition();
    }

    // =========================
    // CAPTURE STRUCTURE (QT ONLY)
    // =========================

    public void CaptureStructure(TruchetRuntime runtime)
    {
        if (!runtime.IsQuadTree)
            return;

        _structure = runtime.CaptureStructure();
        _structureData = SerializeStructure(_structure);
    }

    // =========================
    // APPLY STRUCTURE (QT ONLY)
    // =========================

    public void ApplyStructure(TruchetRuntime runtime)
    {
        if (!runtime.IsQuadTree)
            return;

        if (_structure.Data == null || _structure.Data.Length == 0)
            _structure = DeserializeStructure(_structureData);

        runtime.ApplyStructure(_structure);
        runtime.RebuildComposition();
    }

    // =========================
    // PROVIDE STRUCTURE (for modifiers)
    // =========================

    public QuadTreeStructureSnapshot GetStructure()
    {
        if (_structure.Data == null && _structureData != null)
            _structure = DeserializeStructure(_structureData);

        return _structure;
    }

    // =========================
    // QUAD FULL SERIALIZATION
    // =========================

    private byte[] SerializeQuad(QuadTreeSnapshot snapshot)
    {
        int count = snapshot.Nodes.Length;

        byte[] bytes = new byte[4 + count * 2];

        bytes[0] = (byte)count;
        bytes[1] = (byte)(count >> 8);
        bytes[2] = (byte)(count >> 16);
        bytes[3] = (byte)(count >> 24);

        int offset = 4;

        for (int i = 0; i < count; i++)
        {
            ushort d = snapshot.Nodes[i].Data;
            bytes[offset++] = (byte)d;
            bytes[offset++] = (byte)(d >> 8);
        }

        return bytes;
    }

    private QuadTreeSnapshot DeserializeQuad(byte[] bytes)
    {
        int count =
            bytes[0] |
            (bytes[1] << 8) |
            (bytes[2] << 16) |
            (bytes[3] << 24);

        var nodes = new QuadNodeSnapshot[count];

        int offset = 4;

        for (int i = 0; i < count; i++)
        {
            ushort d = (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
            nodes[i] = new QuadNodeSnapshot { Data = d };
            offset += 2;
        }

        return new QuadTreeSnapshot { Nodes = nodes };
    }

    // =========================
    // STRUCTURE SERIALIZATION
    // =========================

    private byte[] SerializeStructure(QuadTreeStructureSnapshot snapshot)
    {
        int byteCount = snapshot.Data.Length;

        byte[] bytes = new byte[4 + byteCount];

        int bits = snapshot.BitCount;

        bytes[0] = (byte)bits;
        bytes[1] = (byte)(bits >> 8);
        bytes[2] = (byte)(bits >> 16);
        bytes[3] = (byte)(bits >> 24);

        System.Array.Copy(snapshot.Data, 0, bytes, 4, byteCount);

        return bytes;
    }

    private QuadTreeStructureSnapshot DeserializeStructure(byte[] bytes)
    {
        int bits =
            bytes[0] |
            (bytes[1] << 8) |
            (bytes[2] << 16) |
            (bytes[3] << 24);

        int byteCount = bytes.Length - 4;

        var data = new byte[byteCount];
        System.Array.Copy(bytes, 4, data, 0, byteCount);

        return new QuadTreeStructureSnapshot
        {
            Data = data,
            BitCount = bits
        };
    }
}