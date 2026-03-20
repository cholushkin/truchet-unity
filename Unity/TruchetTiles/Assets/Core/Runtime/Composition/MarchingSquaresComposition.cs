// TODO ROADMAP:
// [x] Composition interface migration (ICompositionResult)
// [ ] Implement discrete field sampling (tile-based mask)
// [ ] Implement marching squares case evaluation
// [ ] Generate mesh (vertices + indices)
// [ ] Add chunked mesh generation
// [ ] Add QuadTree adaptive sampling
// [ ] Add LOD support

using UnityEngine;

namespace Truchet
{
    /// <summary>
    /// Composition strategy that converts layout topology
    /// into continuous surface geometry using the Marching Squares algorithm.
    ///
    /// ARCHITECTURAL ROLE:
    /// ---------------------
    /// Composition layer only.
    /// Produces mesh data (no rendering).
    ///
    /// IMPORTANT:
    /// - Does NOT use textures
    /// - Does NOT use SDF
    /// - Will operate on discrete tile mask / field
    ///
    /// FUTURE PIPELINE:
    /// Layout
    ///   ↓
    /// Discrete Field (0/1 or scalar)
    ///   ↓
    /// Marching Squares
    ///   ↓
    /// MeshCompositionResult
    /// </summary>
    public class MarchingSquaresComposition : ICompositionStrategy
    {
        public ICompositionResult Compose(
            object layout,
            TileSet[] tileSets,
            int resolution)
        {
            // 🔴 Not implemented yet by design
            // This is a placeholder to keep architecture consistent

            Debug.LogWarning(
                "MarchingSquaresCompositionStrategy is not implemented yet.");

            return new MeshCompositionResult(
                mesh: null,
                resolution: resolution,
                bounds: new Bounds(Vector3.zero, Vector3.zero),
                fromSdf: false
            );
        }
    }
}