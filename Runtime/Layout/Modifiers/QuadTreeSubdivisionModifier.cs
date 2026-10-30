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
// [ ] Add editor validation for region bounds
// [ ] Add non-rectangular region support

using GameLib.Random;
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
    public class QuadTreeSubdivisionModifier : LayoutModifier
    {
        [Header("Subdivision")]
        [SerializeField, Range(0f, 1f)]
        private float _subdivideProbability = 0.5f;
        
        [SerializeField] private int[] _allowedRotations = { 0, 1, 2, 3 };

        [SerializeField]
        private int _maxDepth = 3;

        // Derived spatial region (not serialized)
        private Vector2 _spatialRegionMin;
        private Vector2 _spatialRegionMax;
        private GameLib.Random.Random _rng;

        public override void Apply(QuadTree layout, GameLib.Random.Random rng)
        {
            if (!enabled)
                return;

            _rng = rng;

            if (!(layout is QuadTree map))
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

                int tileIndex = _rng.Range(0, _tileSet.tiles.Length);
                int rotation = _rng.FromArray(_allowedRotations);

                map.SetTileByNode(index, TileSetId, tileIndex, rotation);
            }
        }

        // --------------------------------------------------
        // Region Mapping
        // --------------------------------------------------

        private void ComputeSpatialRegion(QuadTree map)
        {
            // reinterpret region as normalized domain (0..1)
            _spatialRegionMin = new Vector2(
                Mathf.Clamp01(_regionMin.x),
                Mathf.Clamp01(_regionMin.y));

            _spatialRegionMax = new Vector2(
                Mathf.Clamp01(_regionMax.x),
                Mathf.Clamp01(_regionMax.y));
        }

        // --------------------------------------------------
        // Recursive Subdivision
        // --------------------------------------------------

        private void SubdivideRecursive(QuadTree map, int nodeIndex)
        {
            var node = map.GetNode(nodeIndex);

            if (!node.IsLeaf)
                return;

            if (node.Level >= _maxDepth)
                return;

            if (!NodeIntersectsRegion(node))
                return;

            if (!_rng.TrySpawnEvent(_subdivideProbability))
            {
                if(nodeIndex == 0)
                    Debug.Log($"fail  state: {_rng.GetState()}");
                return;
            }

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

        private bool NodeInsideRegion(QuadNode node)
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

        private bool NodeIntersectsRegion(QuadNode node)
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