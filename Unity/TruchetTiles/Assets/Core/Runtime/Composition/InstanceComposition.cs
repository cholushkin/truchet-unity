// TODO ROADMAP:
// [x] Simplified composition (no interfaces)
// [x] Strongly typed layout entry points
// [x] Unified tile instance output
// [ ] Move builders into dedicated logical builders
// [ ] Add bounds calculation
// [ ] Add chunked composition
// [ ] Add job system support

using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    /// <summary>
    /// Converts layout data into logical tile instances.
    ///
    /// This is the ONLY composition step in the system.
    /// Produces a unified representation for all rendering backends.
    /// </summary>
    public static class InstanceComposition
    {
        // --------------------------------------------------
        // GRID ENTRY POINT
        // --------------------------------------------------

        public static List<TileInstance> Build(
            IGridLayout grid,
            TileSet[] tileSets,
            int resolution)
        {
            List<TileInstance> instances = new List<TileInstance>();

            float tileSize = 1f;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    GridCell cell = grid.GetCell(x, y);

                    if (!IsValid(cell, tileSets))
                        continue;

                    instances.Add(new TileInstance
                    {
                        Position = new Vector2(
                            x + 0.5f,
                            y + 0.5f),
                        Size = tileSize,
                        TileSetId = cell.TileSetId,
                        TileIndex = cell.TileIndex,
                        Rotation = cell.Rotation,
                        Level = 0
                    });
                }
            }

            Debug.Log($"[Compose/Grid] Total instances: {instances.Count}");

            return instances;
        }

        // --------------------------------------------------
        // HIERARCHICAL ENTRY POINT
        // --------------------------------------------------

        public static List<TileInstance> Build(
            IHierarchicalLayout hierarchical,
            TileSet[] tileSets,
            int resolution)
        {
            List<TileInstance> instances = new List<TileInstance>();

            foreach (int index in hierarchical.GetLeafIndices())
            {
                var node = hierarchical.GetNode(index);

                if (!node.IsActive)
                    continue;

                if (!IsValid(node, tileSets))
                    continue;

                float size = node.Size * resolution;

                instances.Add(new TileInstance
                {
                    Position = new Vector2(
                        (node.X + node.Size * 0.5f) * resolution,
                        (node.Y + node.Size * 0.5f) * resolution),
                    Size = size,
                    TileSetId = node.TileSetId,
                    TileIndex = node.TileIndex,
                    Rotation = node.Rotation,
                    Level = node.Level
                });
            }

            Debug.Log($"[Compose/Hierarchy] Total instances: {instances.Count}");

            return instances;
        }

        // --------------------------------------------------
        // VALIDATION
        // --------------------------------------------------

        private static bool IsValid(GridCell cell, TileSet[] tileSets)
        {
            if (cell.TileSetId < 0 || cell.TileSetId >= tileSets.Length)
                return false;

            var set = tileSets[cell.TileSetId];

            if (set == null || set.tiles == null)
                return false;

            if (cell.TileIndex < 0 || cell.TileIndex >= set.tiles.Length)
                return false;

            return set.tiles[cell.TileIndex] != null;
        }

        private static bool IsValid(QuadNode node, TileSet[] tileSets)
        {
            if (node.TileSetId < 0 || node.TileSetId >= tileSets.Length)
                return false;

            var set = tileSets[node.TileSetId];

            if (set == null || set.tiles == null)
                return false;

            if (node.TileIndex < 0 || node.TileIndex >= set.tiles.Length)
                return false;

            return set.tiles[node.TileIndex] != null;
        }
    }
}