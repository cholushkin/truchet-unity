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
}