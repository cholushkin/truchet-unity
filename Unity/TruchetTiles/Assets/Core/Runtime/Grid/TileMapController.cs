// TODO ROADMAP:
// [x] LayoutMode selection (Regular / QuadTree)
// [x] Renderer selection per layout type
// [x] Support QuadTreeSubdivisionModifierRandom
// [ ] Deterministic global seed support
// [ ] Regenerate button (Editor & Runtime)
// [ ] Abstract modifier interface for both layout types
// [ ] GPU renderer switch (InstancedIndirect)
// [ ] Parity-aware rendering integration
// [ ] Inspector layout settings struct
// [ ] Live preview in editor (ExecuteInEditMode)
// [ ] Layout serialization support
// [ ] Runtime rebuild without reallocating textures
// [ ] Chunked rendering for large outputs
// [ ] Frustum-aware hierarchical rendering
// [ ] Debug overlay toggle for node levels

using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

namespace Truchet
{
    public enum LayoutMode
    {
        RegularGrid,
        QuadTree
    }

    public class TileMapController : MonoBehaviour
    {
        [Header("Layout")] [SerializeField] private LayoutMode _layoutMode = LayoutMode.QuadTree;
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 8;

        [Header("Rendering")] [SerializeField] private int _tileResolution = 256;
        [SerializeField] private Renderer _targetRenderer;
        [SerializeField] private bool _debugLines;

        [Button]
        private void Start()
        {
            switch (_layoutMode)
            {
                case LayoutMode.RegularGrid:
                    RenderRegularGrid();
                    break;

                case LayoutMode.QuadTree:
                    RenderQuadTree();
                    break;
            }
        }

        // --------------------------------------------------
        // Regular Grid Path
        // --------------------------------------------------

        private void RenderRegularGrid()
        {
            RegularGridTileMap map =
                new RegularGridTileMap(_width, _height);

            TileMapModifier[] modifiers =
                GetComponents<TileMapModifier>();

            List<TileSet> tileSets = new List<TileSet>();

            for (int i = 0; i < modifiers.Length; i++)
            {
                var mod = modifiers[i];

                if (mod.TileSet == null)
                    continue;

                mod.TileSetId = tileSets.Count;
                tileSets.Add(mod.TileSet);
            }

            foreach (var mod in modifiers)
            {
                if (mod.enabled)
                    mod.Apply(map);
            }

            TextureGridRenderer renderer =
                new TextureGridRenderer(_tileResolution);

            Texture2D result =
                renderer.Render(map, tileSets.ToArray(), _debugLines);

            _targetRenderer.material.mainTexture = result;
        }

        // --------------------------------------------------
        // QuadTree Path
        // --------------------------------------------------

        private async void RenderQuadTree()
        {
            QuadTreeTileMap map = new QuadTreeTileMap(1f);

            var modifiers =
                GetComponents<QuadTreeSubdivisionModifierRandom>();

            List<TileSet> tileSets = new List<TileSet>();

            for (int i = 0; i < modifiers.Length; i++)
            {
                var mod = modifiers[i];

                if (mod.TileSet == null)
                    continue;

                mod.TileSetId = tileSets.Count;
                tileSets.Add(mod.TileSet);
            }

            foreach (var mod in modifiers)
            {
                if (mod.enabled)
                    mod.Apply(map);
            }

            var renderer = new HierarchicalTextureRenderer(
                _tileResolution * _width,
                debugStep: false, // enable debug
                debugDelayMs: 200, // 200ms per tile
                waitForKey: true // or true for manual stepping
            );

            int resolution = _tileResolution * _width;

            Texture2D progressiveTexture =
                new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);

            // IMPORTANT: assign BEFORE rendering starts
            _targetRenderer.material.mainTexture = progressiveTexture;

            await renderer.RenderAsync(
                map,
                tileSets.ToArray(),
                progressiveTexture);
        }
    }
}