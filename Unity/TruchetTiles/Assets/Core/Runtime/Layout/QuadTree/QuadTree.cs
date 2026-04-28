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
    public class QuadTree
    {
        internal struct QuadNode
        {
            public float X;
            public float Y;
            public float Size;
            public int Level;

            public bool IsLeaf;
            public bool IsActive;

            public int ChildIndex;
            public int ParentIndex;

            public int TileSetId;
            public int TileIndex;
            public int Rotation;
        }

        private readonly List<QuadNode> _nodes = new List<QuadNode>();
        private readonly Stack<int> _freeBlocks = new Stack<int>();
        public int NodeCount => _nodes.Count;
        internal List<QuadNode> Nodes => _nodes;
        internal Stack<int> FreeBlocks => _freeBlocks;
        
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

        public int FreeBlockCount => _freeBlocks.Count;

        public QuadTree(float size = 1f)
        {
            _nodes.Add(new QuadNode
            {
                X = 0f,
                Y = 0f,
                Size = size,
                Level = 0,
                IsLeaf = true,
                IsActive = true,
                ChildIndex = -1,
                ParentIndex = -1,
                TileSetId = -1,
                TileIndex = -1,
                Rotation = 0
            });
        }

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

            CreateChild(childStart + 0, node, 0f, 0f, half, level, nodeIndex);
            CreateChild(childStart + 1, node, half, 0f, half, level, nodeIndex);
            CreateChild(childStart + 2, node, 0f, half, half, level, nodeIndex);
            CreateChild(childStart + 3, node, half, half, half, level, nodeIndex);
        }

        public void Collapse(int nodeIndex)
        {
            ValidateNodeIndex(nodeIndex);

            var node = _nodes[nodeIndex];

            if (!node.IsActive)
                return;

            if (!node.IsLeaf)
            {
                int childStart = node.ChildIndex;

                // collapse children first
                for (int i = 0; i < 4; i++)
                {
                    int ci = childStart + i;
                    Collapse(ci);

                    var child = _nodes[ci];
                    child.IsActive = false;
                    _nodes[ci] = child;
                }

                _freeBlocks.Push(childStart);
            }

            node.IsLeaf = true;
            node.ChildIndex = -1;

            _nodes[nodeIndex] = node;
        }

        private void CreateChild(int index, QuadNode parent,
            float offsetX, float offsetY, float size, int level, int parentIndex)
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
                ParentIndex = parentIndex,
                TileSetId = parent.TileSetId,
                TileIndex = parent.TileIndex,
                Rotation = parent.Rotation
            };
        }

        private int AllocateChildBlock()
        {
            if (_freeBlocks.Count > 0)
                return _freeBlocks.Pop();

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

        public Truchet.QuadNode GetNode(int nodeIndex)
        {
            ValidateNodeIndex(nodeIndex);

            var n = _nodes[nodeIndex];

            return new Truchet.QuadNode
            {
                X = n.X,
                Y = n.Y,
                Size = n.Size,
                Level = n.Level,
                IsLeaf = n.IsLeaf,
                IsActive = n.IsActive,
                ChildIndex = n.ChildIndex,
                ParentIndex = n.ParentIndex,
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

        public int FindLeafAt(float u, float v)
        {
            int index = 0;

            while (true)
            {
                var node = _nodes[index];

                if (!node.IsActive)
                    return -1;

                if (node.IsLeaf)
                    return index;

                float half = node.Size * 0.5f;

                bool right = u >= node.X + half;
                bool top = v >= node.Y + half;

                int childOffset =
                    (!right && !top) ? 0 :
                    ( right && !top) ? 1 :
                    (!right &&  top) ? 2 : 3;

                index = node.ChildIndex + childOffset;
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

        private void ValidateNodeIndex(int index)
        {
            if (index < 0 || index >= _nodes.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
        }
        
        internal void ClearInternal()
        {
            _nodes.Clear();
            _freeBlocks.Clear();
        }
    }
}