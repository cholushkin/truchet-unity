// TODO ROADMAP:
// [x] Polymorphic render instruction model
// [x] Rectangle, Ellipse, Pie, Bezier instructions
// [ ] Add LineInstruction
// [ ] Add PolygonInstruction
// [ ] Add GradientInstruction
// [ ] Add Anti-aliased rasterization support

using UnityEngine;

namespace Truchet
{
    internal abstract class TileRenderInstruction
    {
    }

    internal sealed class RectangleInstruction : TileRenderInstruction
    {
        public Vector2 Center;
        public Vector2 Size;
    }

    internal sealed class EllipseInstruction : TileRenderInstruction
    {
        public Vector2 Center;
        public Vector2 Size;
    }

    internal sealed class PieInstruction : TileRenderInstruction
    {
        public Vector2 Center;
        public Vector2 Size;
        public float StartAngle;
        public float SweepAngle;
    }

    internal sealed class BezierInstruction : TileRenderInstruction
    {
        public Vector2 P0;
        public Vector2 P1;
        public Vector2 P2;
        public Vector2 P3;
        public float Thickness;
    }
}