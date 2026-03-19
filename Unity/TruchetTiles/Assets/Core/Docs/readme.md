# Truchet Core — GPU-Driven Procedural Tile System

## Overview

Truchet Core is a modular, GPU-driven tile composition system designed for:

- Large-scale procedural tiling (100k+ tiles)
- Hierarchical QuadTree layouts
- Multi-tileset rendering
- GPU instanced rendering
- Future mesh-based composition (Marching Squares)
- Deterministic structural behavior

The system enforces strict separation between:

Topology → Composition → Rendering

- Topology defines where tiles exist  
- Composition defines how layout is interpreted  
- Rendering defines how results are drawn  

---

# Architectural Layers

## 1. Layout Layer (Topology)

Responsible only for spatial structure and tile indexing.

Supported layouts:
- Regular Grid
- Adaptive QuadTree

Features:
- Stable node indices
- Deterministic subdivision and collapse
- Mutable structure
- No rendering logic

---

## 2. Composition Layer

Transforms layout into renderer-agnostic results.

Interface:
ITileCompositionStrategy

Output:
ICompositionResult

### Instance Composition (Primary)

Layout → InstanceCompositionResult → GPU Renderer

### Mesh Composition (Planned)

Layout → Discrete Field → Marching Squares → Mesh

---

## 3. Rendering Layer

Consumes composition results and draws them.

Interface:
IRenderBackend

### GPU Instanced Backend

- DrawMeshInstancedIndirect
- StructuredBuffer
- Texture2DArray sampling

---

# Texture Array System

Tiles are uploaded as Texture2DArray via TileSetGPUResourceManager.

Supports:
- Multi-tileset merging
- Global motif indexing
- Hash-based caching

---

# Summary

Truchet Core is a scalable system for procedural tile generation with GPU-driven rendering and clean architecture.
