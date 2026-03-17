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
        [SerializeField] private int _tileSizePixels = 1;   // 🔥 Keep small for now
        [SerializeField] private Material _gpuMaterial;

        private ITileCompositionStrategy _compositionStrategy;
        private IRenderBackend _renderBackend;

        private Texture2DArray _tileArray;

        // 🔥 Cache instances
        private List<TileInstanceGPU> _instances;
        private int _resolution;

        private void Start()
        {
            if (_gpuMaterial == null)
            {
                Debug.LogError("GPU Material not assigned.");
                return;
            }

            _compositionStrategy =
                new MotifInstanceCompositionStrategy();

            _renderBackend =
                new GPUInstancedRenderBackend(_gpuMaterial);

            Generate();
        }

        private void Update()
        {
            if (_instances == null || _instances.Count == 0)
                return;

            _renderBackend.RenderInstances(_instances, _resolution);
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

            _resolution = _width * _tileSizePixels;

            _instances =
                _compositionStrategy.ComposeInstances(
                    map,
                    tileSets,
                    _resolution);

            Debug.Log("Generated instances: " + _instances.Count);
        }

        private void GenerateQuadTree()
        {
            QuadTreeTileMap map =
                new QuadTreeTileMap(1f);

            var (tileSets, modifiers) = CollectModifiers();

            foreach (var mod in modifiers)
                mod.Apply(map);

            _resolution = _width * _tileSizePixels;

            _instances =
                _compositionStrategy.ComposeInstances(
                    map,
                    tileSets,
                    _resolution);

            Debug.Log("Generated instances: " + _instances.Count);
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

            // Build Texture2DArray for first tileset only (temporary)
            if (tileSets.Count > 0)
            {
                _tileArray = TileSetTextureArrayBuilder.Build(tileSets[0]);
                _renderBackend.SetTileTextureArray(_tileArray);
            }

            return (tileSets.ToArray(), modifiers);
        }
    }
}