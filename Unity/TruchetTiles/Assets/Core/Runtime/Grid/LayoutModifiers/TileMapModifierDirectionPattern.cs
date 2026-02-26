// TODO ROADMAP:
// [x] Direction pattern modifier
// [x] Multi-line rotation pattern
// [ ] Add validation warnings
// [ ] Add tile index strategy
// [ ] Add connectivity awareness

using NaughtyAttributes;
using UnityEngine;

namespace Truchet
{
    public class TileMapModifierDirectionPattern : TileMapModifier
    {
        [ResizableTextArea]
        [SerializeField] private string _rotationPattern;

        public override void Apply(RegularGridTileMap map)
        {
            if (!enabled)
                return;
            
            if (map == null || _tileSet == null || _tileSet.tiles == null)
                return;

            if (string.IsNullOrWhiteSpace(_rotationPattern))
                return;

            string[] rows = _rotationPattern
                .Replace("\r", "")
                .Split('\n');

            for (int y = 0; y < map.Height; y++)
            {
                if (rows.Length == 0)
                    break;

                string rowPattern = rows[y % rows.Length];

                if (string.IsNullOrEmpty(rowPattern))
                    continue;

                for (int x = 0; x < map.Width; x++)
                {
                    char c = rowPattern[x % rowPattern.Length];

                    int rotation = DirectionToRotation(c);

                    int tileIndex = x % _tileSet.tiles.Length;

                    map.SetTile(x, y, TileSetId, tileIndex, rotation);
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