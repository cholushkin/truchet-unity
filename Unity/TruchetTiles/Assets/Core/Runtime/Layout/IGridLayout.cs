// TODO ROADMAP:
// [x] Define grid abstraction interface
// [x] Add SetTile support
// [ ] Add coordinate helpers
// [ ] Add neighbor query helpers
// [ ] Add bounds validation helpers
// [ ] Add serialization support if needed

using UnityEngine;

namespace Truchet
{
    /// <summary>
    /// Pure topology abstraction.
    /// Defines spatial indexing without rendering concerns.
    /// </summary>
    public interface IGridLayout
    {
        int Width { get; }
        int Height { get; }

        bool IsValid(int x, int y);

        GridCell GetCell(int x, int y);

        void SetTile(int x, int y, int tileSetId, int tileIndex, int rotation);
    }
}