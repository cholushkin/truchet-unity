// TODO ROADMAP:
// [x] Replace custom Perlin with Unity Mathf.PerlinNoise
// [x] Implement seed offset system
// [x] Implement fBm (multi-octave)
// [ ] Add AnimationCurve mapping support
// [ ] Add domain warping option
// [ ] Make Burst-compatible version

using System;
using UnityEngine;

namespace Truchet.Tiles
{
    public sealed class PerlinTileSelectionStrategy : ITileSelectionStrategy
    {
        private readonly TileType[] tileTypes;

        private readonly float baseFrequency;
        private readonly float baseAmplitude;
        private readonly int octaves;

        private readonly float seedOffsetX;
        private readonly float seedOffsetY;

        public PerlinTileSelectionStrategy(
            int seed,
            float frequency,
            float amplitude,
            int octaves)
        {
            tileTypes = (TileType[])Enum.GetValues(typeof(TileType));

            baseFrequency = frequency;
            baseAmplitude = amplitude;
            this.octaves = octaves;

            // Convert seed into deterministic 2D offsets
            unchecked
            {
                seedOffsetX = (seed * 0.1234567f) % 10000f;
                seedOffsetY = (seed * 0.7654321f) % 10000f;
            }
        }

        public TileType SelectTile(int x, int y, int level)
        {
            float noiseValue = 0f;

            float frequency = baseFrequency;
            float amplitude = baseAmplitude;

            for (int i = 0; i < octaves; i++)
            {
                float sampleX = (x + seedOffsetX) * frequency;
                float sampleY = (y + seedOffsetY) * frequency;

                float perlin = Mathf.PerlinNoise(sampleX, sampleY);

                noiseValue += perlin * amplitude;

                frequency *= 2f;
                amplitude *= 0.5f;
            }

            // Clamp to [0,1]
            noiseValue = Mathf.Clamp01(noiseValue);

            int index = Mathf.FloorToInt(noiseValue * (tileTypes.Length - 1));

            return tileTypes[index];
        }
    }
}