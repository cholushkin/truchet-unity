// TODO ROADMAP:
// [x] Direction pattern modifier
// [x] Multi-line rotation pattern
// [ ] Add validation warnings
// [ ] Add tile index strategy
// [ ] Add connectivity awareness

using NaughtyAttributes;
using UnityEngine;
using Random = GameLib.Random.Random;

namespace Truchet
{
    public class DirectionPatternModifier : LayoutModifier
    {
        [ResizableTextArea]
        [SerializeField] private string _rotationPattern;

        public override void Apply(QuadTree layout, Random rng)
        {
            if (!enabled)
                return;

            if (_tileSet == null || _tileSet.tiles == null)
                return;

            if (string.IsNullOrWhiteSpace(_rotationPattern))
                return;

            string[] rows = _rotationPattern
                .Replace("\r", "")
                .Split('\n');

            if (rows.Length == 0)
                return;

            int startX = 0; int  startY = 0;
            int endX = 1; int endY = 1;

            for (int y = startY; y < endY; y++)
            {
                string rowPattern = rows[y % rows.Length];

                if (string.IsNullOrEmpty(rowPattern))
                    continue;

                for (int x = startX; x < endX; x++)
                {
                    char c = rowPattern[x % rowPattern.Length];

                    int rotation = DirectionToRotation(c);
                    int tileIndex = x % _tileSet.tiles.Length;

                    int nodeIndex = layout.FindLeafAt(
                        (x + 0.5f) / (float)(endX - startX),
                        (y + 0.5f) / (float)(endY - startY)
                    );

                    if (nodeIndex >= 0)
                    {
                        layout.SetTileByNode(nodeIndex, TileSetId, tileIndex, rotation);
                    }
                }
            }
        }

        private int DirectionToRotation(char c)
        {
            switch (char.ToUpperInvariant(c))
            {
                case 'L': return 0;
                case 'U': return 1;
                case 'R': return 2;
                case 'D': return 3;
                default: return 0;
            }
        }
    }
}