// TODO ROADMAP:
// [x] Instance generation only (GPU ready)
// [x] Multi-tileset support (global motif index)
// [ ] Add frustum culling
// [ ] Add burst-compatible path

using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    public class QuadTreeInstanceBuilder
    {
        private readonly int _canvasResolution;

        public QuadTreeInstanceBuilder(int canvasResolution)
        {
            _canvasResolution = canvasResolution;
        }

        public List<TileInstanceGPU> BuildInstances(
            IHierarchicalLayout layout,
            TileSet[] tileSets,
            int resolution,
            Dictionary<int, int> tileSetOffsets)
        {
            List<TileInstanceGPU> instances = new List<TileInstanceGPU>();

            foreach (int index in layout.GetLeafIndices())
            {
                QuadNode node = layout.GetNode(index);

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

                int offset = tileSetOffsets[node.TileSetId];

                instances.Add(new TileInstanceGPU
                {
                    transform = matrix,
                    motifIndex = (uint)(offset + node.TileIndex),
                    level = (uint)node.Level
                });
            }

            return instances;
        }

        private bool IsValidNode(QuadNode node, TileSet[] tileSets)
        {
            if (node.TileSetId < 0 ||
                node.TileSetId >= tileSets.Length)
                return false;

            TileSet tileSet = tileSets[node.TileSetId];

            if (node.TileIndex < 0 ||
                node.TileIndex >= tileSet.tiles.Length)
                return false;

            return tileSet.tiles[node.TileIndex] != null;
        }
    }
}