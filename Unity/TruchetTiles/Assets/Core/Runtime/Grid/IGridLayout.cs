// TODO ROADMAP:
// [x] Define grid abstraction interface
// [ ] Add coordinate helpers
// [ ] Add neighbor query helpers
// [ ] Add bounds validation helpers
// [ ] Add serialization support if needed

using UnityEngine;

namespace Truchet.Grid
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
    }
}
