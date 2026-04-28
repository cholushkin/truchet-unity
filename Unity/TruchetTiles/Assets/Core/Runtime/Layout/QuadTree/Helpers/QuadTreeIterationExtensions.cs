// TODO ROADMAP:
// [x] Basic leaf iteration helpers
// [x] Filtered iteration
// [ ] Region-based iteration
// [ ] Depth-based iteration
// [ ] BFS / DFS traversal
// [ ] Parallel iteration support

using System;
using System.Collections.Generic;

namespace Truchet
{
    public static class QuadTreeIterationExtensions
    {
        // --------------------------------------------------
        // BASIC ITERATION
        // --------------------------------------------------

        public static IEnumerable<int> EnumerateLeaves(this QuadTree quad)
        {
            foreach (int index in quad.GetLeafIndices())
                yield return index;
        }

        public static IEnumerable<QuadNode> EnumerateLeafNodes(this QuadTree quad)
        {
            foreach (int index in quad.GetLeafIndices())
                yield return quad.GetNode(index);
        }

        // --------------------------------------------------
        // FILTERED ITERATION
        // --------------------------------------------------

        public static IEnumerable<int> EnumerateLeaves(
            this QuadTree quad,
            Func<QuadNode, bool> predicate)
        {
            foreach (int index in quad.GetLeafIndices())
            {
                var node = quad.GetNode(index);

                if (predicate(node))
                    yield return index;
            }
        }

        public static IEnumerable<QuadNode> EnumerateLeafNodes(
            this QuadTree quad,
            Func<QuadNode, bool> predicate)
        {
            foreach (int index in quad.GetLeafIndices())
            {
                var node = quad.GetNode(index);

                if (predicate(node))
                    yield return node;
            }
        }

        // --------------------------------------------------
        // ACTION HELPERS (no allocations)
        // --------------------------------------------------

        public static void ForEachLeaf(
            this QuadTree quad,
            Action<int, QuadNode> action)
        {
            foreach (int index in quad.GetLeafIndices())
            {
                var node = quad.GetNode(index);
                action(index, node);
            }
        }

        public static void ForEachLeaf(
            this QuadTree quad,
            Func<QuadNode, bool> predicate,
            Action<int, QuadNode> action)
        {
            foreach (int index in quad.GetLeafIndices())
            {
                var node = quad.GetNode(index);

                if (!predicate(node))
                    continue;

                action(index, node);
            }
        }
    }
}