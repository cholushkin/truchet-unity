// TODO ROADMAP:
// [x] Base class for stackable layout modifiers
// [x] TileSet ownership per modifier
// [ ] Add execution order override support
// [ ] Add enable/disable runtime toggle
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

        public abstract void Apply(RegularGridTileMap map);
    }
}