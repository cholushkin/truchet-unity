// TODO ROADMAP:
// [x] Modifier-driven layout controller
// [x] TileSet owned by layout
// [ ] Add runtime regeneration button
// [ ] Add deterministic seed support
// [ ] Add multiscale layout swap
// [ ] Add editor preview support

using UnityEngine;

namespace Truchet
{
    /// <summary>
    /// Creates layout, applies stacked modifiers, then renders.
    /// </summary>
    public class GridLayoutController : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 8;
        [SerializeField] private int _tileResolution = 256;

        [Header("Tile Source")]
        [SerializeField] private TileSet _tileSet;

        [Header("Rendering")]
        [SerializeField] private Renderer _targetRenderer;

        private void Start()
        {
            if (_tileSet == null)
            {
                Debug.LogError("GridLayoutController: TileSet missing.");
                return;
            }

            if (_targetRenderer == null)
            {
                Debug.LogError("GridLayoutController: TargetRenderer missing.");
                return;
            }

            // Create layout with TileSet
            RegularGridLayout layout =
                new RegularGridLayout(_width, _height, _tileSet);

            // Apply stacked modifiers
            ApplyModifiers(layout);

            // Render
            TextureGridRenderer renderer =
                new TextureGridRenderer(_tileResolution);

            Texture2D result = renderer.Render(layout);

            _targetRenderer.material.mainTexture = result;
        }

        private void ApplyModifiers(RegularGridLayout layout)
        {
            GridLayoutModifierBehaviour[] modifiers =
                GetComponents<GridLayoutModifierBehaviour>();

            foreach (var modifier in modifiers)
            {
                if (modifier.enabled)
                    modifier.Apply(layout);
            }
        }
    }
}
