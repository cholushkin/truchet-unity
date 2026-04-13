using UnityEngine;
using System.Collections.Generic;
using GameLib.Random;
using NaughtyAttributes;
using UnityEngine.Serialization;
using Random = GameLib.Random.Random;


// TODO: add patch system (seed + overrides)
// TODO: add brush tools (radius editing)
// TODO: add multi-layer tile support
// TODO: add GPU picking path
// TODO: add undo/redo system
// TODO: add tile weighting / probabilities
// TODO: add debug visualization for quad depth
// TODO: split modifiers into structure vs tile passes
// TODO: add deterministic tile-only generation mode (no structure rebuild)
// TODO: add seed animation / timeline system
// TODO: add partial regeneration (tiles only)
// TODO: add rule-based tile placement system


namespace Truchet
{
    public class TruchetRuntime : MonoBehaviour
    {
        public enum LayoutMode
        {
            RegularGrid,
            QuadTree
        }

        [SerializeField] private LayoutMode _layoutMode = LayoutMode.RegularGrid;
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 8;

        [SerializeField] private TruchetRenderBehaviour _renderBehaviour;
        [SerializeField] private CellRenderDbgOverlay _overlay;

        [AddRandomizeButton] public uint RootSeed;

        private Random _rng;

        private IGridLayout _gridLayout;
        private IHierarchicalLayout _hierarchicalLayout;

        private TileSet[] _tileSets;
        private List<TileInstance> _instances;

        public TilemapStateController StateController;

        public TileSet[] TileSets => _tileSets;

        public bool IsGrid => _gridLayout != null;
        public bool IsQuadTree => _hierarchicalLayout != null;

        public int Width => _width;
        public int Height => _height;

        public IGridLayout GetGridLayout() => _gridLayout;
        public IHierarchicalLayout GetHierarchicalLayout() => _hierarchicalLayout;

        public void SetGridLayout(IGridLayout grid)
        {
            _gridLayout = grid;
            _hierarchicalLayout = null;
        }

        public void SetHierarchicalLayout(IHierarchicalLayout layout)
        {
            _hierarchicalLayout = layout;
            _gridLayout = null;
        }

        private void Start()
        {
            Generate();
        }

        // =====================================================
        // GENERATION ENTRY
        // =====================================================

        [Button]
        public void Generate()
        {
            Debug.Log("--------------------------------------------------");

            // --------------------------------------------------
            // 1. FULL STATE (tiles + structure)
            // --------------------------------------------------
            if (StateController != null && StateController.HasState())
            {
                Debug.Log(
                    $"[Truchet] GENERATE → FULL STATE RESTORE\n" +
                    $"Seed: {RootSeed}\n" +
                    $"Mode: Snapshot (structure + tiles)"
                );

                StateController.Apply(this);
                return;
            }

            // --------------------------------------------------
            // 2. STRUCTURE ONLY (baked topology)
            // --------------------------------------------------
            if (StateController != null && StateController.HasStructure())
            {
                Debug.Log(
                    $"[Truchet] GENERATE → STRUCTURE + TILE SEED\n" +
                    $"Seed: {RootSeed}\n" +
                    $"Mode: Baked Structure (tiles randomized only)"
                );

                StateController.ApplyStructure(this);
                return;
            }

            // --------------------------------------------------
            // 3. FULL PROCEDURAL
            // --------------------------------------------------
            Debug.Log(
                $"[Truchet] GENERATE → FULL PROCEDURAL\n" +
                $"Seed: {RootSeed}\n" +
                $"Mode: Seed drives structure + tiles"
            );

            _rng = RandomHelper.CreateStatefulRandomNumberGenerator(ref RootSeed);

            if (_layoutMode == LayoutMode.RegularGrid)
            {
                Debug.Log($"[Truchet] Layout: RegularGrid ({_width}x{_height})");
                GenerateRegularGrid(_rng);
            }
            else
            {
                Debug.Log($"[Truchet] Layout: QuadTree ({_width}x{_height})");
                GenerateQuadTree(_rng);
            }
        }
        
        [Button("Dump QuadTree")]
        public void DumpQuadTree()
        {
            Debug.Log("--------------------------------------------------");

            if (_hierarchicalLayout == null)
            {
                Debug.LogWarning("[Truchet] DumpQuadTree → No hierarchical layout present.");
                return;
            }

            var quad = _hierarchicalLayout as QuadTree;

            if (quad == null)
            {
                Debug.LogWarning("[Truchet] DumpQuadTree → Current layout is not a QuadTree.");
                return;
            }

            if (quad.NodeCount == 0)
            {
                Debug.LogWarning("[Truchet] DumpQuadTree → QuadTree has no nodes.");
                return;
            }

            Debug.Log(
                $"[Truchet] DumpQuadTree → Nodes: {quad.NodeCount}, Leafs: {quad.LeafCount}, " +
                $"Uniform: {quad.IsUniformDepth}, Depth: {quad.UniformDepth}"
            );

            quad.DebugPrintTree();
        }

        public void ReinitRng()
        {
            _rng = RandomHelper.CreateStatefulRandomNumberGenerator(ref RootSeed);
        }

        // =====================================================
        // TILE GENERATION (DECOUPLED PHASE)
        // =====================================================

        public void FillTiles(IHierarchicalLayout layout)
        {
            if (layout == null)
                return;

            var quad = layout as QuadTree;
            if (quad == null)
                return;

            foreach (int node in quad.GetLeafIndices())
            {
                var (setId, tileIndex, rot) = GetRandomTile();
                quad.SetTileByNode(node, setId, tileIndex, rot);
            }
        }

        public (int setId, int tileIndex, int rot) GetRandomTileForState()
        {
            return GetRandomTile();
        }

        private (int setId, int tileIndex, int rot) GetRandomTile()
        {
            int setId = _rng.Range(0, _tileSets.Length);
            var set = _tileSets[setId];

            if (set.tiles == null || set.tiles.Length == 0)
                return (0, 0, 0);

            int tileIndex = _rng.Range(0, set.tiles.Length);
            int rot = _rng.Range(0, 4);

            return (setId, tileIndex, rot);
        }

        // =====================================================
        // GENERATION MODES
        // =====================================================

        private void GenerateRegularGrid(Random rng)
        {
            var map = new RegularGrid(_width, _height);

            SetGridLayout(map);

            var (tileSets, modifiers) = CollectModifiers();
            _tileSets = tileSets;

            foreach (var mod in modifiers)
                mod.Apply(map, rng);

            RebuildComposition();
        }

        private void GenerateQuadTree(Random rng)
        {
            var map = new QuadTree(1f, _width, _height);

            SetHierarchicalLayout(map);

            var (tileSets, modifiers) = CollectModifiers();
            _tileSets = tileSets;

            foreach (var mod in modifiers)
                mod.Apply(map, rng);

            RebuildComposition();
        }

        // =====================================================
        // INTERACTION
        // =====================================================

        public void ModifyAtUV(Vector2 uv, TileInteractionController.InteractionMode mode)
        {
            if (_gridLayout != null)
                ModifyGrid(uv, mode);
            else if (_hierarchicalLayout != null)
                ModifyQuadTree(uv, mode);

            RebuildComposition();
        }

        private string FormatTile(int setId, int tileIndex, int rot)
        {
            return $"TS:{setId}:{tileIndex}:{rot}";
        }

        private (int, int, int) SetRandomNode(QuadTree quad, int nodeIndex)
        {
            var (setId, tileIndex, rot) = GetRandomTile();
            quad.SetTileByNode(nodeIndex, setId, tileIndex, rot);

            Debug.Log($"RND TILE: {FormatTile(setId, tileIndex, rot)}");

            return (setId, tileIndex, rot);
        }

        private void ModifyGrid(Vector2 uv, TileInteractionController.InteractionMode mode)
        {
            uv.x = Mathf.Clamp01(uv.x);
            uv.y = Mathf.Clamp01(uv.y);

            int x = Mathf.Min(Mathf.FloorToInt(uv.x * _gridLayout.Width), _gridLayout.Width - 1);
            int y = Mathf.Min(Mathf.FloorToInt(uv.y * _gridLayout.Height), _gridLayout.Height - 1);

            if (!_gridLayout.IsValid(x, y))
                return;

            if (mode == TileInteractionController.InteractionMode.Random)
            {
                var (setId, tileIndex, rot) = GetRandomTile();
                _gridLayout.SetTile(x, y, setId, tileIndex, rot);
            }

            if (mode == TileInteractionController.InteractionMode.Erase)
            {
                _gridLayout.SetTile(x, y, -1, -1, 0);
            }

            if (mode == TileInteractionController.InteractionMode.Turn)
            {
                var cell = _gridLayout.GetCell(x, y);
                int newRot = (cell.Rotation + 1) & 3;
                _gridLayout.SetTile(x, y, cell.TileSetId, cell.TileIndex, newRot);
            }
        }

        private void ModifyQuadTree(Vector2 uv, TileInteractionController.InteractionMode mode)
        {
            var quad = (QuadTree)_hierarchicalLayout;

            int nodeIndex = quad.FindLeafAt(uv.x, uv.y);
            if (nodeIndex < 0)
                return;

            switch (mode)
            {
                case TileInteractionController.InteractionMode.Random:
                    SetRandomNode(quad, nodeIndex);
                    break;

                case TileInteractionController.InteractionMode.Split:
                {
                    quad.Subdivide(nodeIndex);

                    var parent = quad.GetNode(nodeIndex);
                    int childStart = parent.ChildIndex;

                    for (int i = 0; i < 4; i++)
                        SetRandomNode(quad, childStart + i);

                    break;
                }

                case TileInteractionController.InteractionMode.Merge:
                {
                    var node = quad.GetNode(nodeIndex);
                    if (node.ParentIndex < 0) return;

                    int parentIndex = node.ParentIndex;
                    quad.Collapse(parentIndex);

                    SetRandomNode(quad, parentIndex);
                    break;
                }

                case TileInteractionController.InteractionMode.Erase:
                    quad.SetTileByNode(nodeIndex, -1, -1, 0);
                    break;

                case TileInteractionController.InteractionMode.Turn:
                {
                    var node = quad.GetNode(nodeIndex);
                    if (!node.IsLeaf) return;

                    int newRot = (node.Rotation + 1) & 3;
                    quad.SetTileByNode(nodeIndex, node.TileSetId, node.TileIndex, newRot);
                    break;
                }
            }
        }

        // =====================================================
        // COMPOSITION
        // =====================================================

        public void RebuildComposition()
        {
            if (_gridLayout != null)
                _instances = InstanceComposition.Build(_gridLayout, _tileSets);
            else if (_hierarchicalLayout != null)
                _instances = InstanceComposition.Build(_hierarchicalLayout, _tileSets);

            Render(_instances, _tileSets);
        }

        private void Render(List<TileInstance> instances, TileSet[] tileSets)
        {
            _renderBehaviour?.Render(instances, tileSets, _width);

            if (_overlay != null)
                _overlay.SetData(instances);
        }

        // =====================================================
        // MODIFIERS
        // =====================================================

        private (TileSet[], LayoutModifier[]) CollectModifiers()
        {
            LayoutModifier[] modifiers = GetComponents<LayoutModifier>();

            List<TileSet> tileSets = new List<TileSet>();

            foreach (var mod in modifiers)
            {
                if (mod.TileSet == null)
                    continue;

                mod.TileSetId = tileSets.Count;
                tileSets.Add(mod.TileSet);
            }

            return (tileSets.ToArray(), modifiers);
        }
    }
}