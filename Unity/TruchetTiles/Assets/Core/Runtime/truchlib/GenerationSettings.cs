// TODO ROADMAP:
// [x] Introduce immutable generation configuration
// [x] Remove CLI dependency concept
// [ ] Add validation layer
// [ ] Add ScriptableObject bridge (Unity layer)
// [ ] Add seed hashing utilities

using System;

namespace Truchet.Tiles
{
    public sealed class GenerationSettings
    {
        public int Seed { get; }
        public int Rows { get; }
        public int Columns { get; }
        public int Levels { get; }

        public bool UsePerlin { get; }

        // Perlin parameters
        public double Frequency { get; }
        public double Amplitude { get; }
        public int Octaves { get; }

        public GenerationSettings(
            int seed,
            int rows,
            int columns,
            int levels,
            bool usePerlin = true,
            double frequency = 4.0,
            double amplitude = 1.0,
            int octaves = 3)
        {
            Seed = seed;
            Rows = rows;
            Columns = columns;
            Levels = levels;

            UsePerlin = usePerlin;

            Frequency = frequency;
            Amplitude = amplitude;
            Octaves = octaves;
        }
    }
}