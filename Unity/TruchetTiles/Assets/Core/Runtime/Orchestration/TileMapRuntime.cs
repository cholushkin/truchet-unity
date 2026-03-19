// TODO ROADMAP:
// [x] Composition result integration
// [x] Multi-tileset GPU resource support
// [x] Clean instance result usage
// [ ] Add regeneration trigger system
// [ ] Add runtime tile updates
// [ ] Add chunk streaming support

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
        [SerializeField] private LayoutMode _layoutMode = LayoutMode.RegularGrid;
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 8;

        [Header("Rendering")]
        [SerializeField] private int _tileSizePixels = 1;
        [SerializeField] private Material _gpuMaterial;

        private ITileCompositionStrategy _compositionStrategy;
        private IRenderBackend _renderBackend;

        private Texture2DArray _tileArray;

        private InstanceCompositionResult _instanceResult;

        private void Start()
        {
            if (_gpuMaterial == null)
            {
                Debug.LogError("GPU Material not assigned.");
                return;
            }

            _compositionStrategy = new MotifInstanceCompositionStrategy();
            _renderBackend = new GPUInstancedRenderBackend(_gpuMaterial);

            Generate();
        }

        private void Update()
        {
            if (_instanceResult == null ||
                _instanceResult.Instances == null ||
                _instanceResult.Instances.Count == 0)
                return;

            _renderBackend.RenderInstances(
                _instanceResult.Instances,
                _instanceResult.Resolution);
        }

        private void Generate()
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

        private void GenerateRegularGrid()
        {
            RegularGridTileMap map =
                new RegularGridTileMap(_width, _height);

            var (tileSets, modifiers) = CollectModifiers();

            foreach (var mod in modifiers)
                mod.Apply(map);

            // 🔥 TEMP DEBUG FILL (CRITICAL)
            if (tileSets.Length > 0 && tileSets[0].tiles.Length > 0)
            {
                for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width; x++)
                {
                    map.SetTile(x, y, 0, 0, 0);
                }
            }

            int resolution = _width * _tileSizePixels;

            var resource = TileSetGPUResourceManager.Build(tileSets);

            if (resource != null)
            {
                _tileArray = resource.TextureArray;
                _renderBackend.SetTileTextureArray(_tileArray);
            }

            _instanceResult =
                (InstanceCompositionResult)_compositionStrategy.Compose(
                    map,
                    tileSets,
                    resolution);
        }

        private void GenerateQuadTree()
        {
            QuadTreeTileMap map =
                new QuadTreeTileMap(1f);

            var (tileSets, modifiers) = CollectModifiers();

            foreach (var mod in modifiers)
                mod.Apply(map);

            int resolution = _width * _tileSizePixels;

            var resource = TileSetGPUResourceManager.Build(tileSets);

            if (resource != null)
            {
                _tileArray = resource.TextureArray;
                _renderBackend.SetTileTextureArray(_tileArray);
            }

            _instanceResult =
                (InstanceCompositionResult)_compositionStrategy.Compose(
                    map,
                    tileSets,
                    resolution);
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