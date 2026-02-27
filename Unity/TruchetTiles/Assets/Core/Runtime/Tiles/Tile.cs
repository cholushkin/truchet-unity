// TODO ROADMAP:
// [x] Runtime tile asset (texture + connectivity)
// [x] PNG-based workflow
// [x] Rotation-aware connectivity helper
// [x] Winged flag
// [ ] Add normal map support
// [ ] Add SDF texture support
// [ ] Add metadata tags
// [ ] Add addressables support

using UnityEngine;

namespace Truchet
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

        [Header("Winged Rendering")]
        public bool IsWinged;

        public string MarchingSquareApproximation;

        public int GetRotatedMask(int rotation)
        {
            return TileTopology.RotateMask(connectivityMask, rotation);
        }
    }
}