// TODO ROADMAP:
// [x] Base class for stackable layout modifiers
// [x] TileSet ownership per modifier
// [x] Add region support (logical grid window)
// [x] Unified Apply(IGridLayout)
// [ ] Add execution order override support
// [ ] Add priority system
// [ ] Add multiscale compatibility

using UnityEngine;
using Random = GameLib.Random.Random;

namespace Truchet
{
    public abstract class LayoutModifier : MonoBehaviour
    {
        [SerializeField] protected TileSet _tileSet;

        internal int TileSetId { get; set; }

        public TileSet TileSet => _tileSet;

        [Header("Region (Logical Grid)")]
        [SerializeField] protected Vector2Int _regionMin = Vector2Int.zero;

        [SerializeField] protected Vector2Int _regionMax =
            new Vector2Int(int.MaxValue, int.MaxValue);

        public abstract void Apply(QuadTree  layout, Random rng);
    }
}