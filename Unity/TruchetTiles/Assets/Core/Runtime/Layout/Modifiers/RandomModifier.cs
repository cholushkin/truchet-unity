// TODO ROADMAP:
// [x] Component-based random layout modifier
// [x] Multi-TileSet support
// [x] Allowed rotation control
// [x] Region support
// [ ] Add deterministic seed support
// [ ] Add weighted tile selection
// [ ] Add adjacency-aware rules

using UnityEngine;
using GameLib.Random;
using Random = GameLib.Random.Random;

namespace Truchet
{
    public class RandomModifier : LayoutModifier
    {
        [SerializeField] private int[] _allowedRotations = { 0, 1, 2, 3 };
        private Random _rng;

        public override void Apply(IGridLayout layout, GameLib.Random.Random rng)
        {
            if (!enabled)
                return;

            if (_allowedRotations == null || _allowedRotations.Length < 1)
                _allowedRotations = new int[] { 0, 1, 2, 3 };
            _rng = rng;

            if (_tileSet == null || _tileSet.tiles == null || _tileSet.tiles.Length == 0)
            {
                Debug.LogWarning("[RandomModifier] Invalid TileSet.");
                return;
            }

            Debug.Log($"[RandomModifier] Tile count: {_tileSet.tiles.Length}");

            GetClampedRegion(layout, out int startX, out int startY, out int endX, out int endY);

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    int tileIndex = _rng.Range(0, _tileSet.tiles.Length);
                    int rotation = GetRotation();

                    layout.SetTile(x, y, TileSetId, tileIndex, rotation);
                }
            }
        }

        private int GetRotation() =>
            _rng.Range(0, _allowedRotations.Length);
    }
}