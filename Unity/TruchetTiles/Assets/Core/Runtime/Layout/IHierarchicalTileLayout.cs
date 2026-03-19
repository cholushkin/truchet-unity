// TODO ROADMAP:
// [x] Hierarchical tile layout interface
// [ ] Add neighbor query helpers
// [ ] Add transform extraction helpers
// [ ] Add bulk iteration API

using System.Collections.Generic;

namespace Truchet
{
    public interface IHierarchicalTileLayout
    {
        int NodeCount { get; }
        int LeafCount { get; }

        QuadNodeInfo GetNode(int nodeIndex);
        IEnumerable<int> GetLeafIndices();

        void Subdivide(int nodeIndex);
        void Collapse(int nodeIndex);

        void SetTileByNode(int nodeIndex, int tileSetId, int tileIndex, int rotation);

        bool IsUniformDepth { get; }
        int UniformDepth { get; }
    }
}