// TODO ROADMAP:
// [x] Component-based random layout modifier
// [x] Multi-TileSet support
// [x] Allowed rotation index control
// [x] Inherit region support from base class
// [ ] Add deterministic seed support
// [ ] Add weighted tile selection
// [ ] Add adjacency-aware randomization
// [ ] Add tile filtering

using UnityEngine;

namespace Truchet
{
    public class TileMapModifierRandom : TileMapModifier
    {
        [SerializeField] private int[] _allowedRotations = { 0, 1, 2, 3 };
        
        
        public override void Apply(IGridLayout layout)
        {
            if (!enabled)
                return;

            if (_tileSet == null || _tileSet.tiles == null)
                return;

            bool useCustomRotations =
                _allowedRotations != null && _allowedRotations.Length > 0;

            GetClampedRegion(layout, out int startX, out int startY, out int endX, out int endY);

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    int tileIndex = Random.Range(0, _tileSet.tiles.Length);

                    int rotation;

                    if (useCustomRotations)
                    {
                        int r = Random.Range(0, _allowedRotations.Length);
                        rotation = Mathf.Clamp(_allowedRotations[r], 0, 3);
                    }
                    else
                    {
                        rotation = Random.Range(0, 4);
                    }

                    layout.SetTile(x, y, TileSetId, tileIndex, rotation);
                }
            }
        }
    }
}