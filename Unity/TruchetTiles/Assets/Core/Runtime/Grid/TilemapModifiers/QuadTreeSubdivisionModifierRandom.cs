// TODO ROADMAP:
// [x] Basic recursive subdivision
// [x] Random subdivision probability
// [x] Leaf tile assignment
// [x] Use inherited grid region mapped to 0..1 spatial region
// [ ] Deterministic seed support
// [ ] Parity-aware tile assignment
// [ ] Adjacency-aware constraints
// [ ] View-dependent subdivision
// [ ] Burst-compatible refactor

using UnityEngine;

namespace Truchet
{
    /// <summary>
    /// Structural QuadTree modifier.
    /// Interprets inherited grid region (_regionMin/_regionMax)
    /// relative to QuadTree logical resolution and maps it into
    /// normalized 0..1 spatial domain.
    ///
    /// QuadTree root always occupies [0,1] x [0,1].
    /// LogicalWidth/LogicalHeight define how grid coordinates
    /// map into that space.
    /// </summary>
    public class QuadTreeSubdivisionModifierRandom : TileMapModifier
    {
        [Header("Subdivision")]
        [SerializeField, Range(0f, 1f)]
        private float _subdivideProbability = 0.5f;

        [SerializeField]
        private int _maxDepth = 3;

        // Derived spatial region (not serialized)
        private Vector2 _spatialRegionMin;
        private Vector2 _spatialRegionMax;

        public override void Apply(IGridLayout layout)
        {
            if (!enabled)
                return;

            if (!(layout is QuadTreeTileMap map))
                return;

            if (_tileSet == null || _tileSet.tiles == null)
                return;

            ComputeSpatialRegion(map);

            SubdivideRecursive(map, 0);

            foreach (int index in map.GetLeafIndices())
            {
                var node = map.GetNode(index);

                if (!NodeInsideRegion(node))
                    continue;

                int tileIndex = Random.Range(0, _tileSet.tiles.Length);
                int rotation = Random.Range(0, 4);

                map.SetTileByNode(index, TileSetId, tileIndex, rotation);
            }
        }

        // --------------------------------------------------
        // Region Mapping
        // --------------------------------------------------

        private void ComputeSpatialRegion(QuadTreeTileMap map)
        {
            int logicalWidth = map.LogicalWidth;
            int logicalHeight = map.LogicalHeight;

            int startX = Mathf.Clamp(_regionMin.x, 0, logicalWidth);
            int startY = Mathf.Clamp(_regionMin.y, 0, logicalHeight);

            int endX = Mathf.Clamp(_regionMax.x, 0, logicalWidth);
            int endY = Mathf.Clamp(_regionMax.y, 0, logicalHeight);

            _spatialRegionMin = new Vector2(
                (float)startX / logicalWidth,
                (float)startY / logicalHeight);

            _spatialRegionMax = new Vector2(
                (float)endX / logicalWidth,
                (float)endY / logicalHeight);
        }

        // --------------------------------------------------
        // Recursive Subdivision
        // --------------------------------------------------

        private void SubdivideRecursive(QuadTreeTileMap map, int nodeIndex)
        {
            var node = map.GetNode(nodeIndex);

            if (!node.IsLeaf)
                return;

            if (node.Level >= _maxDepth)
                return;

            if (!NodeIntersectsRegion(node))
                return;

            if (Random.value > _subdivideProbability)
                return;

            map.Subdivide(nodeIndex);

            node = map.GetNode(nodeIndex);
            int childStart = node.ChildIndex;

            if (childStart < 0)
                return;

            for (int i = 0; i < 4; i++)
            {
                SubdivideRecursive(map, childStart + i);
            }
        }

        // --------------------------------------------------
        // Region Tests
        // --------------------------------------------------

        private bool NodeInsideRegion(QuadNodeInfo node)
        {
            float minX = node.X;
            float minY = node.Y;
            float maxX = node.X + node.Size;
            float maxY = node.Y + node.Size;

            return
                minX >= _spatialRegionMin.x &&
                minY >= _spatialRegionMin.y &&
                maxX <= _spatialRegionMax.x &&
                maxY <= _spatialRegionMax.y;
        }

        private bool NodeIntersectsRegion(QuadNodeInfo node)
        {
            float minX = node.X;
            float minY = node.Y;
            float maxX = node.X + node.Size;
            float maxY = node.Y + node.Size;

            return !(
                maxX <= _spatialRegionMin.x ||
                minX >= _spatialRegionMax.x ||
                maxY <= _spatialRegionMin.y ||
                minY >= _spatialRegionMax.y
            );
        }
    }
}