using UnityEngine;

namespace Truchet
{
    // Centralized transform builder
    public static class TileMatrixBuilder
    {
        public static Matrix4x4 Build(
            Vector2 center,
            float size,
            int rotation90)
        {
            float rotation = rotation90 * 90f;

            return Matrix4x4.TRS(
                new Vector3(center.x, center.y, 0f),
                Quaternion.Euler(0f, 0f, rotation),
                new Vector3(size, size, 1f)
            );
        }
    }
}