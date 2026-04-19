// TODO ROADMAP:
// [x] Convert logical TileInstance → TileInstanceGPU
// [x] Normalize → pixel space conversion
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
            Dictionary<int, int> tileSetOffsets,
            float resolution)
        {
            List<TileInstanceGPU> result =
                new List<TileInstanceGPU>(instances.Count);

            foreach (var inst in instances)
            {
                if (inst.TileIndex < 0)
                    continue;
                
                if (!tileSetOffsets.TryGetValue(inst.TileSetId, out int offset))
                    continue;

                // NORMALIZED → PIXEL SPACE
                Vector2 center = inst.Position * resolution;
                float size     = inst.Size * resolution;

                Matrix4x4 matrix =
                    TileMatrixBuilder.Build(
                        center,
                        size,
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