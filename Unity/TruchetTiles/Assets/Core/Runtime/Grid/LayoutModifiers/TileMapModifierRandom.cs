// TODO ROADMAP:
// [x] Component-based random layout modifier
// [x] Multi-TileSet support
// [ ] Add deterministic seed support
// [ ] Add weighted tile selection
// [ ] Add adjacency-aware randomization
// [ ] Add tile filtering
// [x] Allowed rotation index control

using UnityEngine;

namespace Truchet
{
    public class TileMapModifierRandom : TileMapModifier
    {
        [SerializeField] private int[] _allowedRotations = { 0, 1, 2, 3 };

        public override void Apply(RegularGridTileMap map)
        {
            if (!enabled)
                return;
            
            if (map == null || _tileSet == null || _tileSet.tiles == null)
                return;

            bool useCustomRotations =
                _allowedRotations != null && _allowedRotations.Length > 0;

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
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

                    map.SetTile(x, y, TileSetId, tileIndex, rotation);
                }
            }
        }
    }
}