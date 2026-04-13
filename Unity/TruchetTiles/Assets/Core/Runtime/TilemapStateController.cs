using Truchet;
using UnityEngine;

public class TilemapStateController : MonoBehaviour
{
    private enum StateType : byte
    {
        None = 0,
        Grid = 1,
        QuadTree = 2
    }

    [SerializeField] private StateType _type;

    [SerializeField] private byte[] _fullData;
    [SerializeField] private byte[] _structureData;

    private GridSnapshot _grid;
    private QuadTreeSnapshot _quad;
    private QuadTreeStructureSnapshot _structure;

    public bool HasState()
    {
        return _fullData != null && _fullData.Length > 0;
    }

    // =========================
    // CAPTURE
    // =========================

    public void Capture(TruchetRuntime runtime)
    {
        if (runtime.IsGrid)
        {
            _type = StateType.Grid;

            var grid = runtime.GetGridLayout();
            _grid = CaptureGrid(grid);
            _fullData = GridSnapshotSerializer.Serialize(_grid);
        }
        else if (runtime.IsQuadTree)
        {
            _type = StateType.QuadTree;

            var quad = (QuadTree)runtime.GetHierarchicalLayout();
            _quad = CaptureQuad(quad);
            _fullData = SerializeQuad(_quad);
        }
    }

    // =========================
    // APPLY
    // =========================

    public void Apply(TruchetRuntime runtime)
    {
        if (_type == StateType.Grid)
        {
            if (_grid.Tiles == null || _grid.Tiles.Length == 0)
                _grid = GridSnapshotSerializer.Deserialize(_fullData);

            var grid = ApplyGrid(_grid);
            runtime.SetGridLayout(grid);
        }
        else if (_type == StateType.QuadTree)
        {
            if (_quad.Nodes == null || _quad.Nodes.Length == 0)
                _quad = DeserializeQuad(_fullData);

            var quad = ApplyQuad(_quad, runtime);
            runtime.SetHierarchicalLayout(quad);
        }

        runtime.RebuildComposition();
    }

    // =========================
    // GRID
    // =========================

    private GridSnapshot CaptureGrid(IGridLayout grid)
    {
        int w = grid.Width;
        int h = grid.Height;

        var tiles = new PackedTile[w * h];

        int i = 0;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var cell = grid.GetCell(x, y);
                tiles[i++] = PackedTile.Encode(
                    cell.TileSetId,
                    cell.TileIndex,
                    cell.Rotation);
            }
        }

        return new GridSnapshot
        {
            Width = (ushort)w,
            Height = (ushort)h,
            Tiles = tiles
        };
    }

    private RegularGrid ApplyGrid(GridSnapshot snapshot)
    {
        var grid = new RegularGrid(snapshot.Width, snapshot.Height);

        int i = 0;

        for (int y = 0; y < snapshot.Height; y++)
        {
            for (int x = 0; x < snapshot.Width; x++)
            {
                snapshot.Tiles[i++].Decode(out int setId, out int tileIndex, out int rot);
                grid.SetTile(x, y, setId, tileIndex, rot);
            }
        }

        return grid;
    }

    // =========================
    // QUADTREE
    // =========================

    private QuadTreeSnapshot CaptureQuad(QuadTree quad)
    {
        return QuadTreeSnapshotSerializer.Capture(
            0,
            node => !quad.GetNode(node).IsLeaf,
            node => quad.GetNode(node).ChildIndex,
            node =>
            {
                var n = quad.GetNode(node);
                return (n.TileSetId, n.TileIndex, n.Rotation);
            });
    }

    private QuadTree ApplyQuad(QuadTreeSnapshot snapshot, TruchetRuntime runtime)
    {
        var quad = new QuadTree(1f, 8, 8);

        QuadTreeSnapshotSerializer.Apply(
            snapshot,
            reset: () => { },
            createRoot: () => 0,
            subdivide: quad.Subdivide,
            firstChild: node => quad.GetNode(node).ChildIndex,
            setTile: (node, setId, tileIndex, rot) =>
                quad.SetTileByNode(node, setId, tileIndex, rot));

        return quad;
    }

    // =========================
    // SERIALIZATION (QUAD)
    // =========================

    private byte[] SerializeQuad(QuadTreeSnapshot snapshot)
    {
        int count = snapshot.Nodes.Length;

        const int NODE_SIZE = 2;
        byte[] bytes = new byte[4 + count * NODE_SIZE];

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
    // STRUCTURE (NOT USED YET)
    // =========================

    public QuadTreeStructureSnapshot GetStructure()
    {
        if (_structure.Data == null && _structureData != null)
            _structure = DeserializeStructure(_structureData);

        return _structure;
    }

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