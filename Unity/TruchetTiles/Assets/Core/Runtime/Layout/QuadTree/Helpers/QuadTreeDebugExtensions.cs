// TODO ROADMAP:
// [x] Extract debug traversal from QuadTree core
// [x] Provide readable tree dump
// [ ] Add depth visualization
// [ ] Add region-filtered debug
// [ ] Add gizmo integration

using UnityEngine;

namespace Truchet
{
    public static class QuadTreeDebugExtensions
    {
        public static void DebugPrintTree(this QuadTree quad)
        {
            DebugPrintNode(quad, 0, "");
        }

        private static void DebugPrintNode(
            QuadTree quad,
            int index,
            string indent)
        {
            if (index < 0 || index >= quad.NodeCount)
                return;

            var n = quad.GetNode(index);

            if (!n.IsActive)
                return;

            string type = n.IsLeaf ? "Leaf" : "Node";

            Debug.Log(
                $"{indent}[{index}] {type} | L{n.Level} | Pos({n.X:F2},{n.Y:F2}) | Size {n.Size:F2} | Child:{n.ChildIndex} | Parent:{n.ParentIndex}"
            );

            if (!n.IsLeaf && n.ChildIndex >= 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    DebugPrintNode(quad, n.ChildIndex + i, indent + "  ");
                }
            }
        }
    }
}