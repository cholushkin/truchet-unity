// TODO ROADMAP:
// [x] Remove System.Drawing dependency
// [x] Replace Image output with draw command model
// [ ] Add Unity renderer backend
// [ ] Support mesh/shader renderer
// [ ] Support animation

using System;

namespace Truchet.Tiles
{
    public enum DrawCommandType
    {
        Rectangle,
        Ellipse,
        Pie
    }

    public struct TileDrawCommand
    {
        public DrawCommandType Type;

        public float X;
        public float Y;
        public float Width;
        public float Height;

        public float StartAngle;   // Only for Pie
        public float SweepAngle;   // Only for Pie

        public bool IsPrimary;     // true = primary color, false = secondary
    }
}