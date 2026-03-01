// TODO ROADMAP:
// [x] Rendering backend abstraction
// [ ] Add mesh-based backend (Marching Squares)
// [ ] Add compute-driven backend
// [ ] Add frustum culling integration
// [ ] Add persistent buffer reuse
// [ ] Add multi-material support

using System.Collections.Generic;

namespace Truchet
{
    public interface IRenderBackend
    {
        void RenderInstances(
            List<TileInstanceGPU> instances,
            int resolution);
    }
}