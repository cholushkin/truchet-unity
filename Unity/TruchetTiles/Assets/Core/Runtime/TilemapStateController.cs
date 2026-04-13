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

    // FULL SNAPSHOT (tiles + structure)
    [SerializeField] private byte[] _fullData;

    // STRUCTURE ONLY (quadtree)
    [SerializeField] private byte[] _structureData;

    // Cached runtime snapshots
    private GridSnapshot _grid;
    private QuadTreeSnapshot _quad;
    private QuadTreeStructureSnapshot _structure;

    // =====================================================
    // PUBLIC API
    // =====================================================

    public bool HasState() => _fullData != null && _fullData.Length > 0;
    public bool HasStructure() => _structureData != null && _structureData.Length > 0;

    // =====================================================
    // CAPTURE
    // =====================================================

    public void Capture(TruchetRuntime runtime)
    {
        if (runtime.IsGrid)
        {
            _type = StateType.Grid;

            var grid = runtime.GetGridLayout();
            _grid = CaptureGrid(grid, runtime.RootSeed);

            _fullData = GridSnapshotSerializer.Serialize(_grid);
        }
        else if (runtime.IsQuadTree)
        {
            _type = StateType.QuadTree;

            var quad = (QuadTree)runtime.GetHierarchicalLayout();

            _quad = CaptureQuad(quad, runtime.RootSeed);
            _fullData = SerializeQuad(_quad);
        }
    }

    // 🆕 STRUCTURE ONLY
    public void CaptureStructure(TruchetRuntime runtime)
    {
        if (!runtime.IsQuadTree)
        {
            Debug.LogWarning("[State] Structure snapshot only valid for QuadTree.");
            return;
        }

        var quad = (QuadTree)runtime.GetHierarchicalLayout();

        _structure = QuadTreeStructureSerializer.Capture(
            0,
            node => !quad.GetNode(node).IsLeaf,
            node => quad.GetNode(node).ChildIndex
        );

        _structureData = SerializeStructure(_structure);

        _type = StateType.QuadTree;

        Debug.Log("[State] QuadTree STRUCTURE captured.");
    }

    // =====================================================
    // APPLY
    // =====================================================

    public void Apply(TruchetRuntime runtime)
    {
        if (_type == StateType.Grid)
        {
            if (_grid.Tiles == null || _grid.Tiles.Length == 0)
                _grid = GridSnapshotSerializer.Deserialize(_fullData);

            runtime.RootSeed = _grid.Seed;
            runtime.ReinitRng();

            var grid = ApplyGrid(_grid);
            runtime.SetGridLayout(grid);
        }
        else if (_type == StateType.QuadTree)
        {
            if (_quad.Nodes == null || _quad.Nodes.Length == 0)
                _quad = DeserializeQuad(_fullData);

            runtime.RootSeed = _quad.Seed;
            runtime.ReinitRng();

            var quad = ApplyQuad(_quad);
            runtime.SetHierarchicalLayout(quad);
        }

        runtime.RebuildComposition();
    }

    // 🆕 APPLY STRUCTURE ONLY (UPDATED PIPELINE)
    public void ApplyStructure(TruchetRuntime runtime)
    {
        if (_type != StateType.QuadTree)
        {
            Debug.LogWarning("[State] No QuadTree structure available.");
            return;
        }

        if (_structure.Data == null || _structure.Data.Length == 0)
            _structure = DeserializeStructure(_structureData);

        // --------------------------------------------------
        // RNG (tile phase only)
        // --------------------------------------------------
        runtime.ReinitRng();

        // --------------------------------------------------
        // Build empty QuadTree using runtime config
        // --------------------------------------------------
        var quad = new QuadTree(1f, runtime.Width, runtime.Height);

        // --------------------------------------------------
        // Apply structure (topology only)
        // --------------------------------------------------
        QuadTreeStructureSerializer.Apply(
            _structure,
            reset: () => { },
            createRoot: () => 0,
            subdivide: quad.Subdivide,
            firstChild: node => quad.GetNode(node).ChildIndex
        );

        // --------------------------------------------------
        // Fill tiles (NEW: decoupled phase)
        // --------------------------------------------------
        runtime.FillTiles(quad);

        // --------------------------------------------------
        // Apply to runtime
        // --------------------------------------------------
        runtime.SetHierarchicalLayout(quad);
        runtime.RebuildComposition();

        Debug.Log("[State] QuadTree STRUCTURE applied.");
    }

    // =====================================================
    // GRID
    // =====================================================

    private GridSnapshot CaptureGrid(IGridLayout grid, uint seed)
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

                int setId = Mathf.Max(0, cell.TileSetId);
                int tileIndex = Mathf.Max(0, cell.TileIndex);

                tiles[i++] = PackedTile.Encode(setId, tileIndex, cell.Rotation);
            }
        }

        return new GridSnapshot
        {
            Width = (ushort)w,
            Height = (ushort)h,
            Tiles = tiles,
            Seed = seed
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

    // =====================================================
    // QUADTREE (FULL)
    // =====================================================

    private QuadTreeSnapshot CaptureQuad(QuadTree quad, uint seed)
    {
        var snap = QuadTreeSnapshotSerializer.Capture(
            0,
            node => !quad.GetNode(node).IsLeaf,
            node => quad.GetNode(node).ChildIndex,
            node =>
            {
                var n = quad.GetNode(node);
                return (n.TileSetId, n.TileIndex, n.Rotation);
            });

        snap.LogicalWidth = quad.LogicalWidth;
        snap.LogicalHeight = quad.LogicalHeight;
        snap.Seed = seed;

        return snap;
    }

    private QuadTree ApplyQuad(QuadTreeSnapshot snapshot)
    {
        var quad = new QuadTree(
            1f,
            snapshot.LogicalWidth,
            snapshot.LogicalHeight);

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

    // =====================================================
    // SERIALIZATION (QUAD FULL)
    // =====================================================

    private byte[] SerializeQuad(QuadTreeSnapshot snapshot)
    {
        int count = snapshot.Nodes.Length;

        byte[] bytes = new byte[16 + count * 2];

        System.Buffer.BlockCopy(System.BitConverter.GetBytes(count), 0, bytes, 0, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(snapshot.LogicalWidth), 0, bytes, 4, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(snapshot.LogicalHeight), 0, bytes, 8, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(snapshot.Seed), 0, bytes, 12, 4);

        int offset = 16;

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
        int count = System.BitConverter.ToInt32(bytes, 0);
        int w = System.BitConverter.ToInt32(bytes, 4);
        int h = System.BitConverter.ToInt32(bytes, 8);
        uint seed = System.BitConverter.ToUInt32(bytes, 12);

        var nodes = new QuadNodeSnapshot[count];

        int offset = 16;

        for (int i = 0; i < count; i++)
        {
            ushort d = (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
            nodes[i] = new QuadNodeSnapshot { Data = d };
            offset += 2;
        }

        return new QuadTreeSnapshot
        {
            Nodes = nodes,
            LogicalWidth = w,
            LogicalHeight = h,
            Seed = seed
        };
    }

    // =====================================================
    // STRUCTURE SERIALIZATION
    // =====================================================

    private byte[] SerializeStructure(QuadTreeStructureSnapshot snapshot)
    {
        int byteCount = snapshot.Data.Length;

        byte[] bytes = new byte[4 + byteCount];

        System.Buffer.BlockCopy(System.BitConverter.GetBytes(snapshot.BitCount), 0, bytes, 0, 4);
        System.Buffer.BlockCopy(snapshot.Data, 0, bytes, 4, byteCount);

        return bytes;
    }

    private QuadTreeStructureSnapshot DeserializeStructure(byte[] bytes)
    {
        int bits = System.BitConverter.ToInt32(bytes, 0);

        int byteCount = bytes.Length - 4;
        var data = new byte[byteCount];

        System.Buffer.BlockCopy(bytes, 4, data, 0, byteCount);

        return new QuadTreeStructureSnapshot
        {
            Data = data,
            BitCount = bits
        };
    }
}