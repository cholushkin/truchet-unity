// TODO ROADMAP:
// [x] Persistent buffer reuse
// [x] Intelligent resize strategy
// [x] Proper bounds handling
// [ ] Add SDF shader integration
// [ ] Add texture array support
// [ ] Add frustum culling
// [ ] Add LOD support

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Truchet
{
    public class GPUInstancedRenderBackend : IRenderBackend, System.IDisposable
    {
        private ComputeBuffer _instanceBuffer;
        private ComputeBuffer _argsBuffer;

        private readonly Mesh _quadMesh;
        private readonly Material _material;

        private int _capacity = 0;

        private static readonly int InstancesID =
            Shader.PropertyToID("_Instances");

        public GPUInstancedRenderBackend(Material material)
        {
            _material = material;
            _quadMesh = GenerateQuadMesh();

            CreateArgsBuffer();
        }

        public void RenderInstances(
            List<TileInstanceGPU> instances,
            int resolution)
        {
            if (instances == null || instances.Count == 0)
                return;

            EnsureCapacity(instances.Count);

            _instanceBuffer.SetData(instances);

            _material.SetBuffer(InstancesID, _instanceBuffer);

            UpdateArgsBuffer(instances.Count);

            Bounds bounds = new Bounds(
                new Vector3(resolution * 0.5f, resolution * 0.5f, 0f),
                new Vector3(resolution, resolution, 10f));

            Graphics.DrawMeshInstancedIndirect(
                _quadMesh,
                0,
                _material,
                bounds,
                _argsBuffer);
        }

        private void EnsureCapacity(int required)
        {
            if (required <= _capacity)
                return;

            int stride = sizeof(float) * 16 + sizeof(uint) * 2;

            _instanceBuffer?.Release();

            _capacity = Mathf.NextPowerOfTwo(required);

            _instanceBuffer =
                new ComputeBuffer(_capacity, stride);
        }

        private void CreateArgsBuffer()
        {
            _argsBuffer = new ComputeBuffer(
                1,
                5 * sizeof(uint),
                ComputeBufferType.IndirectArguments);
        }

        private void UpdateArgsBuffer(int instanceCount)
        {
            uint[] args = new uint[5]
            {
                _quadMesh.GetIndexCount(0),
                (uint)instanceCount,
                _quadMesh.GetIndexStart(0),
                _quadMesh.GetBaseVertex(0),
                0
            };

            _argsBuffer.SetData(args);
        }

        private Mesh GenerateQuadMesh()
        {
            Mesh mesh = new Mesh();

            mesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3( 0.5f, -0.5f, 0),
                new Vector3( 0.5f,  0.5f, 0),
                new Vector3(-0.5f,  0.5f, 0)
            };

            mesh.uv = new Vector2[]
            {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(1,1),
                new Vector2(0,1)
            };

            mesh.triangles = new int[]
            {
                0,1,2,
                0,2,3
            };

            mesh.RecalculateBounds();

            return mesh;
        }

        public void Dispose()
        {
            _instanceBuffer?.Release();
            _argsBuffer?.Release();
        }
    }
}