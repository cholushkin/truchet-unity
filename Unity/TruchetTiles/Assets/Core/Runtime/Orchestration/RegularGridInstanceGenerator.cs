// TODO ROADMAP:
// [x] Instance generation only (GPU ready)
// [x] Multi-tileset support (global motif index)
// [ ] Add multiscale support
// [ ] Add chunked instance generation

using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    public class RegularGridInstanceGenerator
    {
        private readonly int _tileSizePixels;

        public RegularGridInstanceGenerator(int tileSizePixels)
        {
            _tileSizePixels = tileSizePixels;
        }

        public List<TileInstanceGPU> GenerateInstances(
            IGridLayout layout,
            TileSet[] tileSets,
            Dictionary<int, int> tileSetOffsets)
        {
            List<TileInstanceGPU> instances = new List<TileInstanceGPU>();

            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    GridCell cell = layout.GetCell(x, y);

                    if (!IsValidCell(cell, tileSets))
                        continue;

                    float tileSize = _tileSizePixels;

                    Vector2 center = new Vector2(
                        x * tileSize + tileSize * 0.5f,
                        y * tileSize + tileSize * 0.5f);

                    Matrix4x4 matrix =
                        TileMatrixBuilder.Build(center, tileSize, cell.Rotation);

                    int offset = tileSetOffsets[cell.TileSetId];

                    instances.Add(new TileInstanceGPU
                    {
                        transform = matrix,
                        motifIndex = (uint)(offset + cell.TileIndex),
                        level = 0
                    });
                }
            }

            return instances;
        }

        private bool IsValidCell(GridCell cell, TileSet[] tileSets)
        {
            if (cell.TileSetId < 0 ||
                cell.TileSetId >= tileSets.Length)
                return false;

            TileSet tileSet = tileSets[cell.TileSetId];

            if (cell.TileIndex < 0 ||
                cell.TileIndex >= tileSet.tiles.Length)
                return false;

            return tileSet.tiles[cell.TileIndex] != null;
        }
    }
}