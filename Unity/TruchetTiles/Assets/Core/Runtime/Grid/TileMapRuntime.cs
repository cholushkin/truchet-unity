// TODO ROADMAP:
// [x] Layout generation
// [x] Composition layer integration
// [x] GPU instanced renderer
// [ ] Add visual mode switch
// [ ] Add SDF shader integration
// [ ] Add marching squares renderer

using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

namespace Truchet
{
    public class TileMapRuntime : MonoBehaviour
    {
        public enum LayoutMode
        {
            RegularGrid,
            QuadTree
        }
        
        [Header("Layout")]
        [SerializeField] private LayoutMode _layoutMode = LayoutMode.QuadTree;
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 8;

        [Header("Rendering")]
        [SerializeField] private int _tileSizePixels = 256;
        [SerializeField] private Material _gpuMaterial;

        private ITileCompositionStrategy _compositionStrategy;
        private IRenderBackend _renderBackend;

        [Button]
        private void Start()
        {
            _compositionStrategy =
                new MotifInstanceCompositionStrategy();

            _renderBackend =
                new GPUInstancedRenderBackend(_gpuMaterial);

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

        private void GenerateRegularGrid()
        {
            RegularGridTileMap map =
                new RegularGridTileMap(_width, _height);

            var (tileSets, modifiers) = CollectModifiers();

            foreach (var mod in modifiers)
                mod.Apply(map);

            int resolution = _width * _tileSizePixels;

            List<TileInstanceGPU> instances =
                _compositionStrategy.ComposeInstances(
                    map,
                    tileSets,
                    resolution);

            _renderBackend.RenderInstances(instances, resolution);
        }

        private void GenerateQuadTree()
        {
            QuadTreeTileMap map = new QuadTreeTileMap(1f);

            var (tileSets, modifiers) = CollectModifiers();

            foreach (var mod in modifiers)
                mod.Apply(map);

            int resolution = _width * _tileSizePixels;

            List<TileInstanceGPU> instances =
                _compositionStrategy.ComposeInstances(
                    map,
                    tileSets,
                    resolution);

            _renderBackend.RenderInstances(instances, resolution);
        }

        private (TileSet[], TileMapModifier[]) CollectModifiers()
        {
            TileMapModifier[] modifiers =
                GetComponents<TileMapModifier>();

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