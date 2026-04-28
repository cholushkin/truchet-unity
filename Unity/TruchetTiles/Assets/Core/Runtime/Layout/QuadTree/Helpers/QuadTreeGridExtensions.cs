// TODO ROADMAP:
// [x] Assure uniform subdivision inside region
// [x] Grid coordinate → node mapping
// [x] Set/Get tile via grid coordinates
// [x] Region-based grid iteration
// [ ] Add collapse-to-grid optimization
// [ ] Add partial resolution support
// [ ] Add bounds-safe sampling modes

using UnityEngine;
using System;

namespace Truchet
{
    public static class QuadTreeGridExtensions
    {
        // --------------------------------------------------
        // ENSURE GRID (UNIFORM DEPTH)
        // --------------------------------------------------

        public static void AssureGrid(
            this QuadTree quad,
            Vector2 min,
            Vector2 max,
            int targetDepth)
        {
            AssureNode(quad, 0, min, max, targetDepth);
        }

        private static void AssureNode(
            QuadTree quad,
            int nodeIndex,
            Vector2 min,
            Vector2 max,
            int targetDepth)
        {
            var node = quad.GetNode(nodeIndex);

            if (!node.IsActive)
                return;

            if (!Intersects(node, min, max))
                return;

            if (node.Level >= targetDepth)
                return;

            if (node.IsLeaf)
                quad.Subdivide(nodeIndex);

            node = quad.GetNode(nodeIndex);
            int childStart = node.ChildIndex;

            for (int i = 0; i < 4; i++)
            {
                AssureNode(quad, childStart + i, min, max, targetDepth);
            }
        }

        // --------------------------------------------------
        // GRID ACCESS
        // --------------------------------------------------

        public static int GetNodeAtGrid(
            this QuadTree quad,
            int x,
            int y,
            Vector2 min,
            Vector2 max,
            int resolution)
        {
            float u = Mathf.Lerp(min.x, max.x, (x + 0.5f) / resolution);
            float v = Mathf.Lerp(min.y, max.y, (y + 0.5f) / resolution);

            return quad.FindLeafAt(u, v);
        }

        public static void SetTileAtGrid(
            this QuadTree quad,
            int x,
            int y,
            Vector2 min,
            Vector2 max,
            int resolution,
            int tileSetId,
            int tileIndex,
            int rotation)
        {
            int node = quad.GetNodeAtGrid(x, y, min, max, resolution);

            if (node >= 0)
            {
                quad.SetTileByNode(node, tileSetId, tileIndex, rotation);
            }
        }

        // --------------------------------------------------
        // GRID ITERATION
        // --------------------------------------------------

        public static void ForEachGridCell(
            this QuadTree quad,
            Vector2 min,
            Vector2 max,
            int resolution,
            Action<int, int, int> action)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int node = quad.GetNodeAtGrid(x, y, min, max, resolution);

                    if (node >= 0)
                        action(x, y, node);
                }
            }
        }

        // --------------------------------------------------
        // HELPERS
        // --------------------------------------------------

        private static bool Intersects(QuadNode node, Vector2 min, Vector2 max)
        {
            float nodeMinX = node.X;
            float nodeMinY = node.Y;
            float nodeMaxX = node.X + node.Size;
            float nodeMaxY = node.Y + node.Size;

            return !(
                nodeMaxX <= min.x ||
                nodeMinX >= max.x ||
                nodeMaxY <= min.y ||
                nodeMinY >= max.y
            );
        }
    }
}