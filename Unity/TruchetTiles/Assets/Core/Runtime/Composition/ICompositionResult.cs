// TODO ROADMAP:
// [x] Base composition result abstraction
// [ ] Add instance result type
// [ ] Add mesh result type
// [ ] Add GPU buffer result type
// [ ] Add field (scalar grid) result type
// [ ] Add streaming / chunked result support

namespace Truchet
{
    /// <summary>
    /// Marker interface for composition outputs.
    ///
    /// Composition layer produces renderer-agnostic results.
    /// Rendering backends consume these results.
    ///
    /// This allows:
    /// - Multiple composition strategies
    /// - Multiple rendering pipelines
    /// - Clean separation between generation and rendering
    /// </summary>
    public interface ICompositionResult
    {
    }
}