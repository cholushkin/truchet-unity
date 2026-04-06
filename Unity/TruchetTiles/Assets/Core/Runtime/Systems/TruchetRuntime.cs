// TODO ROADMAP:
// [x] Removed composition abstraction
// [x] Direct tile instance generation
// [x] GPU render pipeline integration
// [ ] Add regeneration trigger system
// [ ] Add runtime tile updates
// [ ] Add chunk streaming support
// [ ] Decouple resource building from pipeline

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
        [SerializeField] private int _tileSizePixels = 64;
        [SerializeField] private Material _gpuMaterial;

        private IRenderBackend _renderBackend;

        private Texture2DArray _tileArray;
        private List<TileInstanceGPU> _gpuInstances;

        private void Start()
        {
            if (_gpuMaterial == null)
            {
                Debug.LogError("GPU Material not assigned.");
                return;
            }

            _renderBackend = new GPUInstancedRenderBackend(_gpuMaterial);

            Generate();
        }

        private void Update()
        {
            if (_gpuInstances == null || _gpuInstances.Count == 0)
                return;

            _renderBackend.RenderInstances(
                _gpuInstances,
                _width * _tileSizePixels);
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
        // REGULAR GRID
        // --------------------------------------------------

        private void GenerateRegularGrid()
        {
            RegularGrid map = new RegularGrid(_width, _height);

            var (tileSets, modifiers) = CollectModifiers();

            foreach (var mod in modifiers)
                mod.Apply(map);

            int resolution = _width * _tileSizePixels;

            var resource = TileArrayBuilder.Build(tileSets);

            if (resource == null)
            {
                Debug.LogError("Failed to build TileArray.");
                return;
            }

            _tileArray = resource.TextureArray;
            _renderBackend.SetTileTextureArray(_tileArray);

            var instances = InstanceComposition.Build(
                map,
                tileSets,
                resolution);

            var builder = new InstanceRenderDataBuilder();

            _gpuInstances = builder.Build(
                instances,
                resource.TileSetOffsets);
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

            int resolution = _width * _tileSizePixels;

            var resource = TileArrayBuilder.Build(tileSets);

            if (resource == null)
            {
                Debug.LogError("Failed to build TileArray.");
                return;
            }

            _tileArray = resource.TextureArray;
            _renderBackend.SetTileTextureArray(_tileArray);

            var instances = InstanceComposition.Build(
                (IGridLayout)map,
                tileSets,
                resolution);

            var builder = new InstanceRenderDataBuilder();

            _gpuInstances = builder.Build(
                instances,
                resource.TileSetOffsets);
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