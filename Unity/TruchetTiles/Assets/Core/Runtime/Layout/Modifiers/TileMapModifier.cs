// TODO ROADMAP:
// [x] Base class for stackable layout modifiers
// [x] TileSet ownership per modifier
// [x] Add region support (logical grid window)
// [x] Unified Apply(IGridLayout)
// [ ] Add execution order override support
// [ ] Add priority system
// [ ] Add multiscale compatibility

using UnityEngine;

namespace Truchet
{
    public abstract class TileMapModifier : MonoBehaviour
    {
        [SerializeField] protected TileSet _tileSet;

        internal int TileSetId { get; set; }

        public TileSet TileSet => _tileSet;

        [Header("Region (Logical Grid)")]
        [SerializeField] protected Vector2Int _regionMin = Vector2Int.zero;

        [SerializeField] protected Vector2Int _regionMax =
            new Vector2Int(int.MaxValue, int.MaxValue);

        protected void GetClampedRegion(
            IGridLayout layout,
            out int startX,
            out int startY,
            out int endX,
            out int endY)
        {
            startX = Mathf.Clamp(_regionMin.x, 0, layout.Width);
            startY = Mathf.Clamp(_regionMin.y, 0, layout.Height);

            endX = Mathf.Clamp(_regionMax.x, 0, layout.Width);
            endY = Mathf.Clamp(_regionMax.y, 0, layout.Height);
        }

        public abstract void Apply(IGridLayout layout);
    }
}