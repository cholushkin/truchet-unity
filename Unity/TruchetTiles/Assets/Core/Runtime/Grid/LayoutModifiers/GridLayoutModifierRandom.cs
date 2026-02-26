// TODO ROADMAP:
// [x] Component-based random layout modifier
// [ ] Add deterministic seed support
// [ ] Add weighted tile selection
// [ ] Add adjacency-aware randomization
// [ ] Add tile filtering

using UnityEngine;

namespace Truchet
{
    /// <summary>
    /// Randomly assigns tiles and rotations to the layout.
    /// </summary>
    public class GridLayoutModifierRandom : GridLayoutModifierBehaviour
    {
        [Header("Tile Source")]
        [SerializeField] private TileSet _tileSet;

        public override void Apply(RegularGridLayout layout)
        {
            if (layout == null || _tileSet == null || _tileSet.tiles == null)
            {
                Debug.LogError("GridLayoutModifierRandom: Invalid input.");
                return;
            }

            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    int tileIndex = Random.Range(0, _tileSet.tiles.Length);
                    int rotation = Random.Range(0, 4);

                    layout.SetTileIndex(x, y, tileIndex, rotation);
                }
            }
        }
    }
}