// TODO: add patch system (seed + overrides)
// TODO: add brush tools (radius editing)
// TODO: add multi-layer tile support
// TODO: add GPU picking path
// TODO: add undo/redo system
// TODO: add tile weighting / probabilities
// TODO: add debug visualization for quad depth

using UnityEngine;
using System.Collections.Generic;
using GameLib.Random;
using NaughtyAttributes;
using Random = GameLib.Random.Random;

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

        [SerializeField] private TileSet _serviceTileSet;

        [AddRandomizeButton]
        public uint RootSeed;

        private Random _rng;

        private IGridLayout _gridLayout;
        private IHierarchicalLayout _hierarchicalLayout;

        private TileSet[] _tileSets;
        private List<TileInstance> _instances;

        public TileSet[] TileSets => _tileSets;

        private void Start()
        {
            Generate();
        }

        [Button]
        public void Generate()
        {
            _rng = RandomHelper.CreateStatefulRandomNumberGenerator(ref RootSeed);

            if (_layoutMode == LayoutMode.RegularGrid)
                GenerateRegularGrid(_rng);
            else
                GenerateQuadTree(_rng);
        }

        private void GenerateRegularGrid(Random rng)
        {
            var map = new RegularGrid(_width, _height);

            _gridLayout = map;
            _hierarchicalLayout = null;

            var (tileSets, modifiers) = CollectModifiers();
            _tileSets = tileSets;

            foreach (var mod in modifiers)
                mod.Apply(map, rng);

            RebuildComposition();
        }

        private void GenerateQuadTree(Random rng)
        {
            var map = new QuadTree(1f, _width, _height);

            _hierarchicalLayout = map;
            _gridLayout = null;

            var (tileSets, modifiers) = CollectModifiers();
            _tileSets = tileSets;

            foreach (var mod in modifiers)
                mod.Apply(map, rng);

            RebuildComposition();
        }

        public void ModifyAtUV(Vector2 uv, TileInteractionController.InteractionMode mode)
        {
            if (_gridLayout != null)
                ModifyGrid(uv, mode);
            else if (_hierarchicalLayout != null)
                ModifyQuadTree(uv, mode);

            RebuildComposition();
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
            int x = Mathf.FloorToInt(uv.x * _gridLayout.Width);
            int y = Mathf.FloorToInt(uv.y * _gridLayout.Height);

            if (!_gridLayout.IsValid(x, y))
                return;

            if (mode == TileInteractionController.InteractionMode.Random)
            {
                var (setId, tileIndex, rot) = GetRandomTile();
                _gridLayout.SetTile(x, y, setId, tileIndex, rot);

                Debug.Log($"RND TILE: {FormatTile(setId, tileIndex, rot)}");
            }

            if (mode == TileInteractionController.InteractionMode.Erase)
            {
                _gridLayout.SetTile(x, y, 0, 0, 0);
                Debug.Log($"ERASE TILE: {FormatTile(0, 0, 0)}");
            }
            
            if (mode == TileInteractionController.InteractionMode.Turn)
            {
                var cell = _gridLayout.GetCell(x, y);

                int newRot = (cell.Rotation + 1) & 3;

                _gridLayout.SetTile(x, y, cell.TileSetId, cell.TileIndex, newRot);

                Debug.Log($"TURN TILE: {FormatTile(cell.TileSetId, cell.TileIndex, newRot)}");
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
                {
                    SetRandomNode(quad, nodeIndex);
                    break;
                }

                case TileInteractionController.InteractionMode.Split:
                {
                    quad.Subdivide(nodeIndex);

                    var parent = quad.GetNode(nodeIndex);
                    int childStart = parent.ChildIndex;

                    string log = "SPLIT TILE: ";

                    for (int i = 0; i < 4; i++)
                    {
                        var (setId, tileIndex, rot) = SetRandomNode(quad, childStart + i);
                        log += FormatTile(setId, tileIndex, rot) + " ";
                    }

                    Debug.Log(log);
                    break;
                }

                case TileInteractionController.InteractionMode.Merge:
                {
                    var node = quad.GetNode(nodeIndex);

                    if (node.ParentIndex < 0)
                        return;

                    int parentIndex = node.ParentIndex;

                    quad.Collapse(parentIndex);

                    var (setId, tileIndex, rot) = GetRandomTile();
                    quad.SetTileByNode(parentIndex, setId, tileIndex, rot);

                    Debug.Log($"MERGE TILE: {FormatTile(setId, tileIndex, rot)}");

                    break;
                }

                case TileInteractionController.InteractionMode.Erase:
                {
                    quad.SetTileByNode(nodeIndex, 0, 0, 0);
                    Debug.Log($"ERASE TILE: {FormatTile(0, 0, 0)}");
                    break;
                }
                
                case TileInteractionController.InteractionMode.Turn:
                {
                    var node = quad.GetNode(nodeIndex);

                    if (!node.IsLeaf)
                        return;

                    int newRot = (node.Rotation + 1) & 3;

                    quad.SetTileByNode(nodeIndex, node.TileSetId, node.TileIndex, newRot);

                    Debug.Log($"TURN TILE: {FormatTile(node.TileSetId, node.TileIndex, newRot)}");

                    break;
                }
            }
        }

        private void RebuildComposition()
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

        private (TileSet[], LayoutModifier[]) CollectModifiers()
        {
            LayoutModifier[] modifiers = GetComponents<LayoutModifier>();

            List<TileSet> tileSets = new List<TileSet>();

            if (_serviceTileSet != null)
                tileSets.Add(_serviceTileSet);

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