# GPU Texture Array System — Truchet Core

## Overview

The Texture Array system is responsible for preparing tile textures
for efficient GPU sampling in instanced rendering.

Instead of binding individual textures per tile, all tiles are packed into:

Texture2DArray

This allows:
- Single draw call
- Fast GPU lookup via index
- No material switching

---

# Core Component

TileSetGPUResourceManager

Responsibilities:

- Merge multiple TileSets
- Build Texture2DArray
- Assign global motif indices
- Cache results

---

# Pipeline

TileSets[]
   ↓
TileSetGPUResourceManager
   ↓
Texture2DArray + Offsets
   ↓
GPUInstancedRenderBackend
   ↓
Shader sampling

---

# Global Motif Index

Each tile instance contains:

motifIndex

This index maps to:

Texture2DArray slice

Mapping is handled via:

TileSetOffsets

Example:

TileSet 0 → offset 0  
TileSet 1 → offset N  

Final index:

offset + tileIndex

---

# Caching

System uses hash-based caching:

- Prevents rebuilding texture arrays every frame
- Rebuild only when TileSets change

---

# Constraints

All textures must:

- Have identical resolution
- Be readable (CPU access required for building)
- Use same format

---

# Shader Access

Shader samples texture using:

SAMPLE_TEXTURE2D_ARRAY(textureArray, sampler, uv, motifIndex)

---

# Benefits

- Extremely fast rendering
- Minimal CPU overhead
- Scales to large tile counts
- Clean separation from layout

---

# Future Improvements

- Async GPU upload
- Partial rebuilds
- Addressables integration

---

# Summary

Texture Array system enables high-performance rendering
by batching all tile textures into a single GPU resource.
