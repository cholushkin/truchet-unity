# Rendering Architecture — Truchet Core

## Overview

Rendering in Truchet Core is fully decoupled from layout and composition.

Pipeline:

Layout → Composition → Rendering

Rendering layer consumes composition results and is responsible ONLY for drawing.

---

# Core Interface

IRenderBackend

Responsibilities:

- Receive composition results
- Upload data to GPU (if needed)
- Issue draw calls

It MUST NOT:

- Access layout directly
- Modify tile data
- Perform composition logic

---

# Current Backend

## GPUInstancedRenderBackend

Pipeline:

InstanceCompositionResult → GPU buffers → DrawMeshInstancedIndirect

Features:

- High performance (100k+ instances)
- Persistent ComputeBuffer
- Indirect rendering
- Texture2DArray sampling

---

# Texture Binding

Tile textures are provided via:

TileSetGPUResourceManager

This builds:

- Texture2DArray
- TileSet → index offsets

Shader uses:

motifIndex → selects texture slice

---

# Future Backends

## MeshRenderBackend

Input:
MeshCompositionResult

Output:
- MeshRenderer or Graphics.DrawMesh

Use cases:
- Marching Squares
- Continuous surfaces

---

## Texture / Quad Backend

Input:
TextureCompositionResult (future)

Output:
- Material on quad

Use cases:
- Preview
- UI
- Baking

---

# Design Rules

- Rendering is stateless per frame
- Buffers are reused
- No CPU-heavy logic
- Composition decides WHAT, renderer decides HOW

---

# Summary

Rendering layer is:

- Replaceable
- Scalable
- GPU-focused
- Fully decoupled
