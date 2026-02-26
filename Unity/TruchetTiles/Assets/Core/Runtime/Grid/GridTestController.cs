// TODO ROADMAP:
// [x] Simple grid test driver
// [ ] Add deterministic seed support
// [ ] Add runtime regeneration button
// [ ] Add adjacency resolver integration
// [ ] Add multiscale grid swap

using UnityEngine;
using Truchet.Tiles;

namespace Truchet.Grid
{
    /// <summary>
    /// Example scene driver for testing TextureGridRenderer.
    /// </summary>
    public class GridTestController : MonoBehaviour
    {
        [Header("References")]
        public TileSet TileSet;

        [Header("Grid Settings")]
        public int Width = 8;
        public int Height = 8;
        public int TileResolution = 256;

        [Header("Rendering")]
        public Renderer TargetRenderer;

        private void Start()
        {
            if (TileSet == null || TargetRenderer == null)
            {
                Debug.LogError("GridTestController: Missing references.");
                return;
            }

            RegularGridLayout layout = new RegularGridLayout(Width, Height);

            FillRandom(layout);

            TextureGridRenderer renderer =
                new TextureGridRenderer(TileSet, TileResolution);

            Texture2D result = renderer.Render(layout);

            ApplyTexture(result);
        }

        private void FillRandom(RegularGridLayout layout)
        {
            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    int tileIndex = Random.Range(0, TileSet.tiles.Length);
                    int rotation = Random.Range(0, 4);

                    layout.SetTileIndex(x, y, tileIndex, rotation);
                }
            }
        }

        private void ApplyTexture(Texture2D texture)
        {
            TargetRenderer.material.mainTexture = texture;
        }
    }
}