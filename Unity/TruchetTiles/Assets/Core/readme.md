# Truchet Core --- GPU-Driven Procedural Tile System

## Overview

Truchet Core is a modular, GPU-driven tile composition system designed
for:

-   Large-scale procedural tiling (100k+ tiles)
-   Hierarchical QuadTree layouts
-   Binary motif-based tile rendering
-   GPU instanced rendering
-   Marching Squares field composition
-   Deterministic structural behavior

The system separates **topology**, **composition**, and **rendering**
into clean architectural layers.

The core philosophy:

Topology → Composition → Rendering

Layout determines **where tiles exist**, composition determines **how
they are interpreted**, and rendering determines **how they are drawn**.

------------------------------------------------------------------------

# Architectural Layers

## 1. Layout Layer (Topology)

Responsible only for **spatial structure and tile indexing**.

Supported layouts:

-   Regular Grid (uniform resolution)
-   Adaptive QuadTree (mixed-depth hierarchy)

Features:

-   Stable node indices
-   Deterministic subdivision and collapse
-   Spatial traversal access
-   Mutable structure
-   No rendering logic
-   No texture ownership

Layout defines **tile placement only**, never visual appearance.

------------------------------------------------------------------------

## 2. Composition Layer

Transforms layout data into **renderable structures**.

Interface:

ITileCompositionStrategy

Composition interprets layout data and produces **renderer-agnostic
results**.

Current strategies:

### Motif Instance Composition

Produces **GPU instance data** for each tile.

Capabilities:

-   GPU instanced tile rendering
-   Multi-scale tile support
-   Rotation-aware transforms
-   Compatible with Grid and QuadTree layouts

### Marching Squares Composition (Planned)

Converts tile layouts into **continuous mesh geometry**.

Pipeline:

Layout → Scalar Field → Marching Squares → Mesh

Capabilities:

-   Continuous surface generation
-   Mesh output independent of tile boundaries
-   LOD compatible
-   QuadTree compatible

Composition layer **never renders**.

------------------------------------------------------------------------

## 3. Rendering Layer (GPU Only)

Responsible for **drawing prepared composition results**.

Interface:

IRenderBackend

Primary backend:

### GPU Instanced Renderer

Features:

-   Graphics.DrawMeshInstancedIndirect
-   StructuredBuffer`<TileInstanceGPU>`{=html}
-   Indirect draw argument buffers
-   Single quad mesh reused for all tiles
-   Texture2DArray motif sampling
-   Minimal CPU overhead

Renderer **never queries layout directly**.

------------------------------------------------------------------------

# Tile Model

A tile is a **visual motif descriptor**.

Tiles do not encode adjacency rules or topology.

Each tile contains:

Tile ├ Texture2D texture └ bool IsWinged

Tiles represent **visual motifs only**.

All spatial logic lives in the **layout layer**.

------------------------------------------------------------------------

# Motif Texture System

Tiles are stored as **binary motif textures**.

Characteristics:

-   Black / white motif shapes
-   Deterministic rasterization
-   Texture2DArray GPU sampling
-   Instanced rendering per tile

Motif textures are generated using the **Tile Cooking pipeline**.

------------------------------------------------------------------------

# Tile Cooking Pipeline

Tiles are procedurally generated from **command scripts**.

Example pipeline:

TileCookDefinition ↓ Instruction Script ↓ Rasterizer ↓ PNG Texture ↓
Tile Asset

Instruction types include:

-   Rectangle
-   Ellipse
-   Pie segments
-   Bezier curves

Advantages:

-   Deterministic tile generation
-   Artist-controlled procedural motifs
-   No manual texture painting required

------------------------------------------------------------------------

# Marching Squares Field System

The system supports generating **continuous mesh surfaces** from tile
layouts.

Pipeline:

Layout → Scalar Field → Threshold → Mesh

Capabilities:

-   Continuous geometry generation
-   Seamless tile blending
-   GPU-friendly mesh output
-   LOD scalable
-   Compatible with QuadTree layouts

Marching Squares operates at the **composition layer**, not the
rendering layer.

------------------------------------------------------------------------

# Multiscale & Hierarchical Support

The system supports hierarchical layouts via **QuadTrees**.

Features:

-   Mixed-depth tiles
-   Adaptive subdivision
-   Level-based scaling
-   Winged tiles for overlap
-   Future view-dependent LOD

This enables efficient rendering of **large procedural tile worlds**.

------------------------------------------------------------------------

# Determinism & Mutability

Core design principles:

-   Stable node identity
-   Explicit subdivision and collapse
-   No implicit rebuilds
-   Deterministic layout mutation
-   Modifiers own procedural generation
-   Layout remains a passive container

Structure only changes through explicit operations.

------------------------------------------------------------------------

# Performance Characteristics

Designed for large-scale procedural rendering:

-   100k+ tile instances
-   GPU instanced rendering
-   Minimal CPU allocations
-   Persistent GPU buffers
-   Indirect drawing
-   Scalable hierarchical layouts

The architecture is designed to remain **GPU-driven and CPU-light**.

------------------------------------------------------------------------

# Final Feature Set

-   GPU instanced tile motif rendering
-   Procedural tile cooking system
-   Regular Grid and QuadTree layouts
-   Composition abstraction layer
-   Marching Squares mesh generation
-   Deterministic topology mutation
-   Winged multiscale tiles
-   Indirect GPU rendering pipeline
-   Extensible rendering backends

------------------------------------------------------------------------

# Summary

Truchet Core is a scalable, modular foundation for:

-   Procedural tiling systems
-   GPU-driven rendering pipelines
-   Hierarchical spatial layouts
-   Large-scale tile worlds

The architecture focuses on **clean separation of topology, composition,
and rendering**, enabling extensibility and high performance for
procedural tile generation.
