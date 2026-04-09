// TODO ROADMAP:
// [x] Simplified composition (no interfaces)
// [x] Strongly typed layout entry points
// [x] Unified tile instance output
// [x] Normalize output (0..1 space)
// [x] Propagate winged tile flag
// [ ] Move builders into dedicated logical builders
// [ ] Add bounds calculation
// [ ] Add chunked composition
// [ ] Add job system support

using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    public static class InstanceComposition
    {
        // --------------------------------------------------
        // GRID ENTRY POINT (NORMALIZED)
        // --------------------------------------------------

        public static List<TileInstance> Build(
            IGridLayout grid,
            TileSet[] tileSets)
        {
            List<TileInstance> instances = new List<TileInstance>();

            float invWidth  = 1f / grid.Width;
            float invHeight = 1f / grid.Height;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    GridCell cell = grid.GetCell(x, y);

                    if (!IsValid(cell, tileSets))
                        continue;

                    var set = tileSets[cell.TileSetId];
                    var tile = set.tiles[cell.TileIndex];

                    instances.Add(new TileInstance
                    {
                        Position = new Vector2(
                            (x + 0.5f) * invWidth,
                            (y + 0.5f) * invHeight),

                        Size = invWidth,

                        TileSetId = cell.TileSetId,
                        TileIndex = cell.TileIndex,
                        Rotation  = cell.Rotation,
                        Level     = 0,

                        IsWinged = tile.IsWinged
                    });
                }
            }

            Debug.Log($"[Compose/Grid] Total instances: {instances.Count}");

            return instances;
        }

        // --------------------------------------------------
        // HIERARCHICAL ENTRY POINT (ALREADY NORMALIZED)
        // --------------------------------------------------

        public static List<TileInstance> Build(
            IHierarchicalLayout hierarchical,
            TileSet[] tileSets)
        {
            List<TileInstance> instances = new List<TileInstance>();

            foreach (int index in hierarchical.GetLeafIndices())
            {
                var node = hierarchical.GetNode(index);

                if (!node.IsActive)
                    continue;

                if (!IsValid(node, tileSets))
                    continue;

                var set = tileSets[node.TileSetId];
                var tile = set.tiles[node.TileIndex];

                instances.Add(new TileInstance
                {
                    Position = new Vector2(
                        node.X + node.Size * 0.5f,
                        node.Y + node.Size * 0.5f),

                    Size = node.Size,

                    TileSetId = node.TileSetId,
                    TileIndex = node.TileIndex,
                    Rotation  = node.Rotation,
                    Level     = node.Level,

                    IsWinged = tile.IsWinged
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