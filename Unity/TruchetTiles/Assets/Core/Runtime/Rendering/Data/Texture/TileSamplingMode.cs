namespace Truchet
{
    // TODO ROADMAP:
    // [x] Unified sampling abstraction
    // [x] Coverage-based rasterization
    // [ ] Add SDF sampling
    // [ ] Add adaptive sampling (quality tiers)
    // [ ] GPU parity implementation

    public enum TileSamplingMode
    {
        Nearest,
        Bilinear,
        Coverage
    }
}