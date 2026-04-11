// TODO ROADMAP:
// [x] Layout → Modifier → Composition → Rendering pipeline
// [x] Support Grid and QuadTree layouts
// [x] Removed resolution dependency from composition
// [x] Overlay integration (debug rendering)
// [ ] Add change detection (avoid full rebuild)
// [ ] Add chunked generation
// [ ] Add async generation option

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

        [Header("Layout")]
        [SerializeField] private LayoutMode _layoutMode = LayoutMode.RegularGrid;
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 8;

        [Header("Rendering")]
        [SerializeField] private TruchetRenderBehaviour _renderBehaviour;

        [Header("Debug Overlay (Optional)")]
        [SerializeField] private CellRenderDbgOverlay _overlay;

        [AddRandomizeButton]
        public uint RootSeed;

        private Random _rng;

        private void Start()
        {
            Generate();
        }

        [Button]
        public void Generate()
        {
            _rng = RandomHelper.CreateStatefulRandomNumberGenerator(ref RootSeed);
            Debug.Log($"Root seed: {RootSeed}");
            
            switch (_layoutMode)
            {
                case LayoutMode.RegularGrid:
                    GenerateRegularGrid(_rng);
                    break;

                case LayoutMode.QuadTree:
                    GenerateQuadTree(_rng);
                    break;
            }
        }

        // --------------------------------------------------
        // GRID
        // --------------------------------------------------

        private void GenerateRegularGrid(Random rng)
        {
            RegularGrid map = new RegularGrid(_width, _height);

            var (tileSets, modifiers) = CollectModifiers();

            foreach (var mod in modifiers)
                mod.Apply(map, rng);

            var instances = InstanceComposition.Build(
                (IGridLayout)map,
                tileSets);

            Render(instances, tileSets);
        }

        // --------------------------------------------------
        // QUADTREE
        // --------------------------------------------------

        private void GenerateQuadTree(Random rng)
        {
            QuadTree map = new QuadTree(
                size: 1f,
                logicalWidth: _width,
                logicalHeight: _height);

            var (tileSets, modifiers) = CollectModifiers();

            foreach (var mod in modifiers)
                mod.Apply(map, rng);

            var instances = InstanceComposition.Build(
                (IHierarchicalLayout)map,
                tileSets);

            Render(instances, tileSets);
        }

        // --------------------------------------------------
        // SHARED RENDER PATH
        // --------------------------------------------------

        private void Render(List<TileInstance> instances, TileSet[] tileSets)
        {
            _renderBehaviour?.Render(instances, tileSets, _width);

            if (_overlay != null)
            {
                _overlay.SetData(instances);
            }
        }

        // --------------------------------------------------
        // HELPERS
        // --------------------------------------------------

        private (TileSet[], LayoutModifier[]) CollectModifiers()
        {
            LayoutModifier[] modifiers =
                GetComponents<LayoutModifier>();

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