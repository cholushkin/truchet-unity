// TODO ROADMAP:
// [x] Instance generation only (GPU ready)
// [ ] Add SDF-aware motif index mapping
// [ ] Add frustum culling support
// [ ] Add burst-compatible path

using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    public class QuadTreeInstanceGenerator
    {
        private readonly int _canvasResolution;

        public QuadTreeInstanceGenerator(int canvasResolution)
        {
            _canvasResolution = canvasResolution;
        }

        public List<TileInstanceGPU> GenerateInstances(
            IHierarchicalTileLayout layout,
            TileSet[] tileSets,
            int resolution)
        {
            List<TileInstanceGPU> instances = new List<TileInstanceGPU>();

            foreach (int index in layout.GetLeafIndices())
            {
                QuadNodeInfo node = layout.GetNode(index);

                if (!node.IsActive)
                    continue;

                if (!IsValidNode(node, tileSets))
                    continue;

                float nodeSizePx = node.Size * resolution;

                Vector2 center = new Vector2(
                    (node.X + node.Size * 0.5f) * resolution,
                    (node.Y + node.Size * 0.5f) * resolution);

                float renderSize = nodeSizePx * 2f;

                Matrix4x4 matrix =
                    TileMatrixBuilder.Build(center, renderSize, node.Rotation);

                instances.Add(new TileInstanceGPU
                {
                    transform = matrix,
                    motifIndex = (uint)node.TileIndex,
                    level = (uint)node.Level
                });
            }

            return instances;
        }

        private bool IsValidNode(QuadNodeInfo node, TileSet[] tileSets)
        {
            if (node.TileSetId < 0 ||
                node.TileSetId >= tileSets.Length)
                return false;

            TileSet tileSet = tileSets[node.TileSetId];

            if (node.TileIndex < 0 ||
                node.TileIndex >= tileSet.tiles.Length)
                return false;

            Tile tile = tileSet.tiles[node.TileIndex];

            return tile != null;
        }
    }
}