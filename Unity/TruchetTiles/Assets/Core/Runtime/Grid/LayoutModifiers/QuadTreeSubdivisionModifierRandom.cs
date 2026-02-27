// TODO ROADMAP:
// [x] Basic recursive subdivision
// [x] Random subdivision probability
// [x] Leaf tile assignment
// [ ] Deterministic seed support
// [ ] Parity-aware tile assignment
// [ ] Adjacency-aware constraints
// [ ] View-dependent subdivision
// [ ] Burst-compatible refactor

using UnityEngine;

namespace Truchet
{
    public class QuadTreeSubdivisionModifierRandom : MonoBehaviour
    {
        [SerializeField] private TileSet _tileSet;

        [Header("Subdivision")]
        [SerializeField, Range(0f, 1f)]
        private float _subdivideProbability = 0.5f;

        [SerializeField]
        private int _maxDepth = 3;

        internal int TileSetId { get; set; }

        public TileSet TileSet => _tileSet;

        public void Apply(QuadTreeTileMap map)
        {
            if (_tileSet == null || _tileSet.tiles == null)
                return;

            SubdivideRecursive(map, 0);

            foreach (int index in map.GetLeafIndices())
            {
                int tileIndex = Random.Range(0, _tileSet.tiles.Length);
                int rotation = Random.Range(0, 4);

                map.SetTileByNode(index, TileSetId, tileIndex, rotation);
            }
        }

        private void SubdivideRecursive(QuadTreeTileMap map, int nodeIndex)
        {
            var node = map.GetNode(nodeIndex);

            if (!node.IsLeaf)
                return;

            if (node.Level >= _maxDepth)
                return;

            if (Random.value > _subdivideProbability)
                return;

            // Perform subdivision
            map.Subdivide(nodeIndex);

            // IMPORTANT: re-fetch updated node
            node = map.GetNode(nodeIndex);

            int childStart = node.ChildIndex;

            // Safety check
            if (childStart < 0)
                return;

            for (int i = 0; i < 4; i++)
            {
                SubdivideRecursive(map, childStart + i);
            }
        }
    }
}