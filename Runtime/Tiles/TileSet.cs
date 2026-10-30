// TODO: add adjacency lookup (by connectivity rules)
// TODO: add weighted random selection
// TODO: add filtered queries (by tags / constraints)
// TODO: add runtime cache for fast lookup
// TODO: add rotation rules per tile
// TODO: add variant groups (tile families)
// TODO: add editor validation (auto-fix empty sets)

using UnityEngine;
using GameLib.Random;
using Random = GameLib.Random.Random;

namespace Truchet
{
    [CreateAssetMenu(
        fileName = "TileSet",
        menuName = "Truchet/Tiles/Tile Set",
        order = 1)]
    public class TileSet : ScriptableObject
    {
        public Tile[] tiles;

        public int Count => tiles != null ? tiles.Length : 0;

        public bool HasTiles => Count > 0;

        public bool IsValidIndex(int index)
        {
            return tiles != null && index >= 0 && index < tiles.Length;
        }

        public Tile Get(int index)
        {
            if (!IsValidIndex(index))
                return null;

            return tiles[index];
        }

        public bool TryGet(int index, out Tile tile)
        {
            if (IsValidIndex(index))
            {
                tile = tiles[index];
                return true;
            }

            tile = null;
            return false;
        }

        public Tile First()
        {
            if (!HasTiles)
                return null;

            return tiles[0];
        }

        public int GetRandomIndex(Random rng)
        {
            if (!HasTiles)
                return 0;

            return rng.Range(0, tiles.Length);
        }

        public Tile GetRandom(Random rng)
        {
            if (!HasTiles)
                return null;

            return tiles[rng.Range(0, tiles.Length)];
        }
    }
}