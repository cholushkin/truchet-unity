// TODO ROADMAP:
// [x] Introduce TileTree rendering abstraction
// [ ] Add Texture2D backend
// [ ] Add Mesh backend
// [ ] Add GPU shader backend
// [ ] Add LOD-aware renderer

namespace Truchet.Rendering
{
    using Truchet.Tiles;

    public interface ITileTreeRenderer
    {
        void Render(TileNode[,] rootNodes);
    }
}