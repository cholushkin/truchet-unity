// TODO ROADMAP:
// [x] TileSet asset (collection of tiles)
// [ ] Add lookup by connectivity
// [ ] Add weighted lookup
// [ ] Add adjacency filtering helpers
// [ ] Add runtime dictionary cache

using UnityEngine;

namespace Truchet
{
    [CreateAssetMenu(
        fileName = "TileSet",
        menuName = "Truchet/Tiles/Tile Set",
        order = 1)]
    public class TileSet : ScriptableObject
    {
        public Tile[] tiles;
    }
}