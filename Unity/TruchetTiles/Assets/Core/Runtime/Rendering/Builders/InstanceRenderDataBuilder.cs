// TODO ROADMAP:
// [x] Convert logical TileInstance → TileInstanceGPU
// [x] Handle TileSet offset mapping
// [ ] Add frustum culling
// [ ] Add LOD filtering
// [ ] Add chunked conversion
// [ ] Add burst/jobified version

using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    public class InstanceRenderDataBuilder
    {
        public List<TileInstanceGPU> Build(
            List<TileInstance> instances,
            Dictionary<int, int> tileSetOffsets)
        {
            List<TileInstanceGPU> result =
                new List<TileInstanceGPU>(instances.Count);

            foreach (var inst in instances)
            {
                if (!tileSetOffsets.TryGetValue(inst.TileSetId, out int offset))
                    continue;

                Matrix4x4 matrix =
                    TileMatrixBuilder.Build(
                        inst.Position,
                        inst.Size,
                        inst.Rotation);

                result.Add(new TileInstanceGPU
                {
                    transform = matrix,
                    motifIndex = (uint)(offset + inst.TileIndex),
                    level = (uint)inst.Level
                });
            }

            return result;
        }
    }
}