using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    public static class InstanceComposition
    {
        // --------------------------------------------------
        // HIERARCHICAL ENTRY POINT
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

                bool isEmpty = IsEmpty(node.TileSetId, node.TileIndex);

                bool isRenderable = !isEmpty && IsRenderable(node, tileSets);

                bool isWinged = false;

                if (isRenderable)
                {
                    var tile = tileSets[node.TileSetId].tiles[node.TileIndex];
                    isWinged = tile.IsWinged;
                }

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

                    IsWinged = isWinged
                });
            }

            Debug.Log($"[Compose/Hierarchy] Total instances: {instances.Count}");

            return instances;
        }

        // --------------------------------------------------
        // SEMANTICS
        // --------------------------------------------------

        private static bool IsEmpty(int setId, int tileIndex)
        {
            return setId < 0 || tileIndex < 0;
        }

        private static bool IsRenderable(QuadNode node, TileSet[] tileSets)
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