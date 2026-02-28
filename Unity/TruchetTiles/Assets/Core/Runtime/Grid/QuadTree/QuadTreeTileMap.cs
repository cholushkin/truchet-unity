// TODO ROADMAP:
// [x] Stable node index QuadTree
// [x] Canonical child order
// [x] Free list reuse
// [x] Spatial traversal grid access
// [x] Uniform depth detection
// [x] Mutable structure
// [ ] Add deterministic random build helper
// [ ] Add adjacency helpers
// [ ] Add debug validation tools

using System;
using System.Collections.Generic;

namespace Truchet
{
    public class QuadTreeTileMap : IGridLayout, IHierarchicalTileLayout
    {
        private struct QuadNode
        {
            public float X;
            public float Y;
            public float Size;
            public int Level;

            public bool IsLeaf;
            public bool IsActive;

            public int ChildIndex;

            public int TileSetId;
            public int TileIndex;
            public int Rotation;
        }

        private readonly List<QuadNode> _nodes = new List<QuadNode>();
        private readonly Stack<int> _freeIndices = new Stack<int>();
        
        private readonly int _logicalWidth;
        private readonly int _logicalHeight;

        private bool _isUniformDepth;
        private int _uniformDepth;

        public int NodeCount => _nodes.Count;
        public int LogicalWidth => _logicalWidth;
        public int LogicalHeight => _logicalHeight;


        public int LeafCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _nodes.Count; i++)
                {
                    if (_nodes[i].IsActive && _nodes[i].IsLeaf)
                        count++;
                }
                return count;
            }
        }

        public bool IsUniformDepth => _isUniformDepth;
        public int UniformDepth => _uniformDepth;

        // --------------------------------------------------

        public QuadTreeTileMap(
            float size = 1f,
            int logicalWidth = 8,
            int logicalHeight = 8)
        {
            _logicalWidth = logicalWidth;
            _logicalHeight = logicalHeight;

            _nodes.Add(new QuadNode
            {
                X = 0f,
                Y = 0f,
                Size = size,
                Level = 0,
                IsLeaf = true,
                IsActive = true,
                ChildIndex = -1,
                TileSetId = -1,
                TileIndex = -1,
                Rotation = 0
            });

            RecalculateUniformState();
        }

        // --------------------------------------------------
        // Subdivision
        // --------------------------------------------------

        public void Subdivide(int nodeIndex)
        {
            ValidateNodeIndex(nodeIndex);

            var node = _nodes[nodeIndex];

            if (!node.IsActive || !node.IsLeaf)
                return;

            float half = node.Size * 0.5f;
            int level = node.Level + 1;

            int childStart = AllocateChildBlock();

            node.IsLeaf = false;
            node.ChildIndex = childStart;
            _nodes[nodeIndex] = node;

            // Canonical order:
            // 0 = bottom-left
            // 1 = bottom-right
            // 2 = top-left
            // 3 = top-right

            CreateChild(childStart + 0, node, 0f, 0f, half, level);
            CreateChild(childStart + 1, node, half, 0f, half, level);
            CreateChild(childStart + 2, node, 0f, half, half, level);
            CreateChild(childStart + 3, node, half, half, half, level);

            RecalculateUniformState();
        }

        public void Collapse(int nodeIndex)
        {
            ValidateNodeIndex(nodeIndex);

            var node = _nodes[nodeIndex];

            if (!node.IsActive || node.IsLeaf)
                return;

            int childStart = node.ChildIndex;

            var child0 = _nodes[childStart + 0];

            node.TileSetId = child0.TileSetId;
            node.TileIndex = child0.TileIndex;
            node.Rotation = child0.Rotation;

            for (int i = 0; i < 4; i++)
            {
                int ci = childStart + i;
                var child = _nodes[ci];
                child.IsActive = false;
                _nodes[ci] = child;
                _freeIndices.Push(ci);
            }

            node.IsLeaf = true;
            node.ChildIndex = -1;
            _nodes[nodeIndex] = node;

            RecalculateUniformState();
        }

        private void CreateChild(int index, QuadNode parent,
            float offsetX, float offsetY, float size, int level)
        {
            EnsureNodeCapacity(index);

            _nodes[index] = new QuadNode
            {
                X = parent.X + offsetX,
                Y = parent.Y + offsetY,
                Size = size,
                Level = level,
                IsLeaf = true,
                IsActive = true,
                ChildIndex = -1,
                TileSetId = parent.TileSetId,
                TileIndex = parent.TileIndex,
                Rotation = parent.Rotation
            };
        }

        private int AllocateChildBlock()
        {
            if (_freeIndices.Count >= 4)
            {
                int a = _freeIndices.Pop();
                int b = _freeIndices.Pop();
                int c = _freeIndices.Pop();
                int d = _freeIndices.Pop();

                int min = Math.Min(Math.Min(a, b), Math.Min(c, d));
                return min;
            }

            int start = _nodes.Count;
            _nodes.Add(default);
            _nodes.Add(default);
            _nodes.Add(default);
            _nodes.Add(default);
            return start;
        }

        private void EnsureNodeCapacity(int index)
        {
            while (_nodes.Count <= index)
                _nodes.Add(default);
        }

        // --------------------------------------------------
        // Hierarchical Interface
        // --------------------------------------------------

        public QuadNodeInfo GetNode(int nodeIndex)
        {
            ValidateNodeIndex(nodeIndex);

            var n = _nodes[nodeIndex];

            return new QuadNodeInfo
            {
                X = n.X,
                Y = n.Y,
                Size = n.Size,
                Level = n.Level,
                IsLeaf = n.IsLeaf,
                IsActive = n.IsActive,
                ChildIndex = n.ChildIndex,
                TileSetId = n.TileSetId,
                TileIndex = n.TileIndex,
                Rotation = n.Rotation
            };
        }

        public IEnumerable<int> GetLeafIndices()
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].IsActive && _nodes[i].IsLeaf)
                    yield return i;
            }
        }

        public void SetTileByNode(int nodeIndex, int tileSetId, int tileIndex, int rotation)
        {
            ValidateNodeIndex(nodeIndex);

            var node = _nodes[nodeIndex];

            if (!node.IsActive || !node.IsLeaf)
                return;

            node.TileSetId = tileSetId;
            node.TileIndex = tileIndex;
            node.Rotation = rotation;

            _nodes[nodeIndex] = node;
        }

        // --------------------------------------------------
        // Grid Interface (Uniform Only)
        // --------------------------------------------------

        public int Width
        {
            get
            {
                EnsureUniform();
                return 1 << _uniformDepth;
            }
        }

        public int Height
        {
            get
            {
                EnsureUniform();
                return 1 << _uniformDepth;
            }
        }

        public bool IsValid(int x, int y)
        {
            if (!_isUniformDepth)
                return false;

            int res = 1 << _uniformDepth;
            return x >= 0 && y >= 0 && x < res && y < res;
        }

        public GridCell GetCell(int x, int y)
        {
            EnsureUniform();

            if (!IsValid(x, y))
                throw new ArgumentOutOfRangeException();

            int nodeIndex = TraverseToLeaf(x, y);

            var node = _nodes[nodeIndex];

            return new GridCell(x, y)
            {
                TileSetId = node.TileSetId,
                TileIndex = node.TileIndex,
                Rotation = node.Rotation
            };
        }

        public void SetTile(int x, int y, int tileSetId, int tileIndex, int rotation)
        {
            EnsureUniform();

            int nodeIndex = TraverseToLeaf(x, y);

            SetTileByNode(nodeIndex, tileSetId, tileIndex, rotation);
        }

        private int TraverseToLeaf(int x, int y)
        {
            int nodeIndex = 0;

            for (int level = _uniformDepth - 1; level >= 0; level--)
            {
                var node = _nodes[nodeIndex];

                if (node.IsLeaf)
                    return nodeIndex;

                int half = 1 << level;

                bool right = x >= half;
                bool top = y >= half;

                int childOffset =
                    (!right && !top) ? 0 :
                    ( right && !top) ? 1 :
                    (!right &&  top) ? 2 : 3;

                if (right) x -= half;
                if (top) y -= half;

                nodeIndex = node.ChildIndex + childOffset;
            }

            return nodeIndex;
        }

        // --------------------------------------------------

        private void RecalculateUniformState()
        {
            int? depth = null;
            bool uniform = true;

            for (int i = 0; i < _nodes.Count; i++)
            {
                var n = _nodes[i];
                if (!n.IsActive || !n.IsLeaf)
                    continue;

                if (depth == null)
                    depth = n.Level;
                else if (depth.Value != n.Level)
                {
                    uniform = false;
                    break;
                }
            }

            _isUniformDepth = uniform && depth.HasValue;
            _uniformDepth = depth ?? 0;
        }

        private void EnsureUniform()
        {
            if (!_isUniformDepth)
                throw new InvalidOperationException(
                    "QuadTreeTileMap is adaptive. IGridLayout not supported.");
        }

        private void ValidateNodeIndex(int index)
        {
            if (index < 0 || index >= _nodes.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}