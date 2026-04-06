// TODO ROADMAP:
// [x] Layout → Modifier → Composition → Rendering pipeline
// [x] Support Grid and QuadTree layouts
// [x] Removed resolution dependency from composition
// [ ] Add change detection (avoid full rebuild)
// [ ] Add chunked generation
// [ ] Add async generation option

using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

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

        private void Start()
        {
            Generate();
        }

        [Button]
        public void Generate()
        {
            switch (_layoutMode)
            {
                case LayoutMode.RegularGrid:
                    GenerateRegularGrid();
                    break;

                case LayoutMode.QuadTree:
                    GenerateQuadTree();
                    break;
            }
        }

        // --------------------------------------------------
        // GRID
        // --------------------------------------------------

        private void GenerateRegularGrid()
        {
            RegularGrid map = new RegularGrid(_width, _height);

            var (tileSets, modifiers) = CollectModifiers();

            foreach (var mod in modifiers)
                mod.Apply(map);

            var instances = InstanceComposition.Build(
                (IGridLayout)map,
                tileSets);

            _renderBehaviour.Render(instances, tileSets, _width);
        }

        // --------------------------------------------------
        // QUADTREE
        // --------------------------------------------------

        private void GenerateQuadTree()
        {
            QuadTree map = new QuadTree(
                size: 1f,
                logicalWidth: _width,
                logicalHeight: _height);

            var (tileSets, modifiers) = CollectModifiers();

            foreach (var mod in modifiers)
                mod.Apply(map);

            var instances = InstanceComposition.Build(
                (IHierarchicalLayout)map,
                tileSets);

            _renderBehaviour.Render(instances, tileSets, _width);
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