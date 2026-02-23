// TODO ROADMAP:
// [x] Introduce tile selection abstraction
// [ ] Implement RandomTileSelectionStrategy
// [ ] Implement PerlinTileSelectionStrategy
// [ ] Support constraint-aware selection
// [ ] Support weighted tile distribution

namespace Truchet.Tiles
{
    public interface ITileSelectionStrategy
    {
        TileType SelectTile(
            int x,
            int y,
            int level);
    }
}