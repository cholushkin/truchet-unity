// TODO ROADMAP:
// [x] Runtime tile asset (texture + connectivity)
// [ ] Add normal map support
// [ ] Add SDF texture support
// [ ] Add GPU index support
// [ ] Add metadata tags

using UnityEngine;

namespace Truchet.Tiles
{
    [CreateAssetMenu(
        fileName = "Tile",
        menuName = "Truchet/Tiles/Tile",
        order = 0)]
    public class Tile : ScriptableObject
    {
        [Tooltip("NESW bitmask")]
        public int connectivityMask;

        public Texture2D texture;
    }
}