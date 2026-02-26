// TODO ROADMAP:
// [x] Runtime tile asset (texture + connectivity)
// [x] PNG-based workflow (texture imported from disk)
// [ ] Add normal map support
// [ ] Add SDF texture support
// [ ] Add metadata tags
// [ ] Add addressables support


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