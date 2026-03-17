using UnityEngine;
using System.Runtime.InteropServices;

namespace Truchet
{
    // GPU-ready instance representation


    [StructLayout(LayoutKind.Sequential)]
    public struct TileInstanceGPU
    {
        public Matrix4x4 transform;
        public uint motifIndex;
        public uint level;

        // padding to 16-byte alignment
        private uint _pad0;
        private uint _pad1;
    }

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