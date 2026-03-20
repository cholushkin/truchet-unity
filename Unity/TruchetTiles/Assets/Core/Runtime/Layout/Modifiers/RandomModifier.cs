// TODO ROADMAP:
// [x] Component-based random layout modifier
// [x] Multi-TileSet support
// [x] Allowed rotation control
// [x] Region support
// [ ] Add deterministic seed support
// [ ] Add weighted tile selection
// [ ] Add adjacency-aware rules

using UnityEngine;

namespace Truchet
{
    public class RandomModifier : LayoutModifier
    {
        [SerializeField] private int[] _allowedRotations = { 0, 1, 2, 3 };

        public override void Apply(IGridLayout layout)
        {
            if (!enabled)
                return;

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
                    int tileIndex = Random.Range(0, _tileSet.tiles.Length);
                    int rotation = GetRotation();

                    layout.SetTile(x, y, TileSetId, tileIndex, rotation);
                }
            }
        }

        private int GetRotation()
        {
            if (_allowedRotations != null && _allowedRotations.Length > 0)
            {
                int index = Random.Range(0, _allowedRotations.Length);
                return Mathf.Clamp(_allowedRotations[index], 0, 3);
            }

            return Random.Range(0, 4);
        }
    }
}