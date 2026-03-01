# Truchet Core --- GPU-Driven Multiscale Tile System

## Overview

Truchet Core is a modular, GPU-driven tile composition system designed
for:

-   Large-scale procedural tiling (100k+ tiles)
-   Hierarchical QuadTree layouts
-   SDF-based tile motifs
-   GPU instanced rendering
-   Marching Squares field composition
-   Deterministic structural behavior

The system separates **topology**, **composition**, and **rendering**
into clean architectural layers.

------------------------------------------------------------------------

## Architectural Layers

### 1. Layout Layer (Topology)

Responsible only for spatial structure and tile indexing.

-   Regular Grid (uniform resolution)
-   Adaptive QuadTree (mixed-depth hierarchy)
-   Stable node indices
-   Deterministic subdivision
-   No rendering logic
-   No texture ownership

Layout defines *where* tiles exist --- not how they are drawn.

------------------------------------------------------------------------

### 2. Composition Layer

Transforms layout data into renderable geometry or instances.

Interface: `ITileCompositionStrategy`

Implementations:

-   **Motif Instanced Composition**
    -   Produces per-tile GPU instance data
    -   Supports winged tiles
    -   Supports multiscale rendering
    -   Level-based parity logic
-   **Marching Squares Composition**
    -   Converts scalar field into mesh geometry
    -   Generates seamless surfaces
    -   Works from SDF or discrete field data
    -   Resolution independent
    -   LOD compatible

Composition layer owns topology interpretation.

------------------------------------------------------------------------

### 3. Rendering Layer (GPU Only)

Responsible only for drawing prepared data.

Interface: `IRenderBackend`

Primary backend:

-   **GPU Instanced Renderer**
    -   Graphics.DrawMeshInstancedIndirect
    -   StructuredBuffer`<TileInstanceGPU>`{=html}
    -   Indirect argument buffers
    -   Single quad mesh for motifs
    -   Shader-driven logic
    -   No CPU texture compositing

Renderer never queries layout directly.

------------------------------------------------------------------------

## Tile Model

Each Tile asset may contain:

-   Connectivity mask (NESW bitmask)
-   SDF representation (preferred)
-   Optional texture fallback
-   Winged flag for multiscale overlap

Tiles are data descriptors --- not rendering logic.

------------------------------------------------------------------------

## SDF-Driven Motif System

Signed Distance Fields enable:

-   Resolution independence
-   Smooth edges
-   Shader-based parity inversion
-   Dynamic animation potential
-   Procedural blending
-   Marching Squares field generation

SDF is the canonical motif representation.

------------------------------------------------------------------------

## Marching Squares Field System

The system supports generating continuous surfaces from tile data:

Layout → Scalar Field → Threshold → Mesh

Capabilities:

-   Continuous geometry generation
-   Seamless blending across tiles
-   GPU-friendly mesh output
-   LOD scalable
-   Compatible with QuadTree hierarchy

Marching Squares operates at composition level, not rendering level.

------------------------------------------------------------------------

## Multiscale & Hierarchical Support

-   True QuadTree structure
-   Mixed-depth rendering
-   Level-based scaling
-   Winged tile overlap support
-   Future frustum-based culling
-   GPU-driven LOD compatibility

------------------------------------------------------------------------

## Determinism & Mutability

-   Stable node identity
-   Explicit subdivision / collapse
-   No implicit rebuilds
-   Modifiers own procedural logic
-   Layout remains passive container

------------------------------------------------------------------------

## Performance Characteristics

Designed for:

-   100k+ tiles
-   Indirect instancing
-   GPU-side motif logic
-   Minimal CPU allocations
-   Burst-compatible future extensions

------------------------------------------------------------------------

## Final Feature Set

-   GPU instanced motif rendering
-   SDF-based tile system
-   Marching Squares surface generation
-   Regular Grid + Adaptive QuadTree layouts
-   Composition abstraction layer
-   Deterministic structural mutation
-   Winged multiscale tiles
-   Shader-driven visual logic
-   Extensible backend architecture

------------------------------------------------------------------------

Truchet Core is a scalable, modular foundation for procedural tiling,
field-based rendering, and multiscale GPU-driven composition.
