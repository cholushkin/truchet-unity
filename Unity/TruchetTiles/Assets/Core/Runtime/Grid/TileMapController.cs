// TODO ROADMAP:
// [x] Modifier-driven layout controller
// [x] Multi-TileSet registry
// [ ] Add runtime regeneration button
// [ ] Add deterministic seed support
// [ ] Add multiscale layout swap
// [ ] Add editor preview support

using UnityEngine;
using System.Collections.Generic;

namespace Truchet
{
    public class TileMapController  : MonoBehaviour
    {
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 8;
        [SerializeField] private int _tileResolution = 256;
        [SerializeField] private Renderer _targetRenderer;
        [SerializeField] private bool _debugLines;

        private void Start()
        {
            RegularGridTileMap map =
                new RegularGridTileMap(_width, _height);

            TileMapModifier[] modifiers =
                GetComponents<TileMapModifier>();

            List<TileSet> tileSets = new List<TileSet>();

            // Build registry
            for (int i = 0; i < modifiers.Length; i++)
            {
                var mod = modifiers[i];

                if (mod.TileSet == null)
                    continue;

                mod.TileSetId = tileSets.Count;
                tileSets.Add(mod.TileSet);
            }

            // Apply modifiers
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
    }
}