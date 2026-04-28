// TODO ROADMAP:
// [x] Restore structure-only generation mode
// [x] Remove dependency on removed hierarchical API
// [ ] Align structure restore with new snapshot system
// [ ] Add validation for snapshot compatibility

using UnityEngine;
using System.Collections.Generic;
using GameLib.Random;
using NaughtyAttributes;
using Random = GameLib.Random.Random;

namespace Truchet
{
    public class TruchetEngine : MonoBehaviour
    {
        [SerializeField] private TruchetRenderBehaviour _renderBehaviour;
        [SerializeField] private CellRenderDbgOverlay _overlay;

        [AddRandomizeButton] public uint RootSeed;

        private Random _rng;
        private QuadTree _quadTree;

        private TileSet[] _tileSets;
        private List<TileInstance> _instances;

        public TilemapStateController StateController;

        public TileSet[] TileSets => _tileSets;

        public QuadTree GetQuadTree() => _quadTree;

        public void SetQuadTree(QuadTree quad)
        {
            _quadTree = quad;
        }

        private void Start()
        {
            Generate();
        }

        [Button]
        public void Generate()
        {
            Debug.Log("--------------------------------------------------");

            // 1. FULL STATE
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

            // 2. STRUCTURE ONLY
            if (StateController != null && StateController.HasStructure())
            {
                Debug.Log(
                    $"[Truchet] GENERATE → STRUCTURE + TILE SEED\n" +
                    $"Seed: {RootSeed}\n" +
                    $"Mode: Baked Structure"
                );

                StateController.ApplyStructure(this);
                return;
            }

            // 3. FULL PROCEDURAL
            Debug.Log(
                $"[Truchet] GENERATE → FULL PROCEDURAL\n" +
                $"Seed: {RootSeed}"
            );

            _rng = RandomHelper.CreateStatefulRandomNumberGenerator(ref RootSeed);

            GenerateQuadTree(_rng);
        }

        [Button("Dump QuadTree")]
        public void DumpQuadTree()
        {
            Debug.Log("--------------------------------------------------");

            if (_quadTree == null)
            {
                Debug.LogWarning("[Truchet] DumpQuadTree → No QuadTree.");
                return;
            }

            if (_quadTree.NodeCount == 0)
            {
                Debug.LogWarning("[Truchet] DumpQuadTree → Empty.");
                return;
            }

            Debug.Log(
                $"[Truchet] Nodes: {_quadTree.NodeCount}, Leafs: {_quadTree.LeafCount}"
            );
        }

        public void ReinitRng()
        {
            _rng = RandomHelper.CreateStatefulRandomNumberGenerator(ref RootSeed);
        }

        public void FillTiles()
        {
            if (_quadTree == null)
                return;

            foreach (int node in _quadTree.GetLeafIndices())
            {
                var (setId, tileIndex, rot) = GetRandomTile();
                _quadTree.SetTileByNode(node, setId, tileIndex, rot);
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

        private void GenerateQuadTree(Random rng)
        {
            var map = new QuadTree(1f);

            SetQuadTree(map);

            var (tileSets, modifiers) = CollectModifiers();
            _tileSets = tileSets;

            foreach (var mod in modifiers)
                mod.Apply(map, rng);

            RebuildComposition();
        }

        public void ModifyAtUV(Vector2 uv, TileInteractionController.InteractionMode mode)
        {
            if (_quadTree != null)
                ModifyQuadTree(uv, mode);

            RebuildComposition();
        }

        private (int, int, int) SetRandomNode(int nodeIndex)
        {
            var (setId, tileIndex, rot) = GetRandomTile();
            _quadTree.SetTileByNode(nodeIndex, setId, tileIndex, rot);

            Debug.Log($"RND TILE: TS:{setId}:{tileIndex}:{rot}");

            return (setId, tileIndex, rot);
        }

        private void ModifyQuadTree(Vector2 uv, TileInteractionController.InteractionMode mode)
        {
            int nodeIndex = _quadTree.FindLeafAt(uv.x, uv.y);
            if (nodeIndex < 0)
                return;

            switch (mode)
            {
                case TileInteractionController.InteractionMode.Random:
                    SetRandomNode(nodeIndex);
                    break;

                case TileInteractionController.InteractionMode.Split:
                {
                    _quadTree.Subdivide(nodeIndex);

                    var parent = _quadTree.GetNode(nodeIndex);
                    int childStart = parent.ChildIndex;

                    for (int i = 0; i < 4; i++)
                        SetRandomNode(childStart + i);

                    break;
                }

                case TileInteractionController.InteractionMode.Merge:
                {
                    var node = _quadTree.GetNode(nodeIndex);
                    if (node.ParentIndex < 0) return;

                    int parentIndex = node.ParentIndex;
                    _quadTree.Collapse(parentIndex);

                    SetRandomNode(parentIndex);
                    break;
                }

                case TileInteractionController.InteractionMode.Erase:
                    _quadTree.SetTileByNode(nodeIndex, -1, -1, 0);
                    break;

                case TileInteractionController.InteractionMode.Turn:
                {
                    var node = _quadTree.GetNode(nodeIndex);
                    if (!node.IsLeaf) return;

                    int newRot = (node.Rotation + 1) & 3;
                    _quadTree.SetTileByNode(nodeIndex, node.TileSetId, node.TileIndex, newRot);
                    break;
                }
            }
        }

        public void RebuildComposition()
        {
            if (_quadTree != null)
                _instances = InstanceComposition.Build(_quadTree, _tileSets);

            Render(_instances, _tileSets);
        }

        private void Render(List<TileInstance> instances, TileSet[] tileSets)
        {
            _renderBehaviour?.Render(instances, tileSets, 1);

            if (_overlay != null)
                _overlay.SetData(instances);
        }

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