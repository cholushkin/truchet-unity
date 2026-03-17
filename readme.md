# Truchet Tiles -- Multiscale & Winged Rendering System

## Overview

This module provides a scalable tile generation and rendering system
designed for:

-   Efficient GPU-driven rendering
-   True hierarchical QuadTree (not flat list)
-   Efficient subdivision logic
-   Clean data-oriented structures
-   Designed for large tile counts
-   Deterministic rebuild behavior
-   Editor-time tile baking pipeline
-   Support for both multiscale winged tiles and regular grids

The system is divided into two primary layers:

1.  Editor-time Tile Cooking
2.  Runtime Placement & Rendering

------------------------------------------------------------------------

# Winged Tile Definition

The direct method uses the "winged" tiles illustrated in Figure 3 of the
original paper.

A winged tile consists of:

-   The content of the tile inside a square boundary
-   "Wings" that extend outside the square to complete the motif
-   The square boundary is conceptual; wings intentionally exceed it
-   The gray background often shown in diagrams is not part of the tile
-   Wing shapes are arbitrary but stylistically consistent within a
    motif set

This allows seamless multiscale composition while maintaining visual
continuity across boundaries.

------------------------------------------------------------------------

# Editor-Time Tile Cooking

## Purpose

Prepare tile assets once, use many times at runtime.

## Pipeline

TileCookDefinition (ScriptableObject) → Command Script (DSL) →
TileCommandParser → TileRenderInstructions → TileTextureCooker → Tile
(Texture + Connectivity)

## Command DSL Example

ELP 0.5 1 0.2 0.2\
ELP 1 0.5 0.2 0.2\
BZR 0.5 1 0.5 0.75 0.75 0.5 1 0.5 0.15

Supported primitives:

-   RCT -- Rectangle
-   ELP -- Ellipse
-   PIE -- Pie slice
-   BZR -- Cubic Bezier with thickness

This enables:

-   Winged tiles
-   Classic Truchet tiles
-   Arbitrary motifs
-   Future SDF export
-   GPU baking in future

------------------------------------------------------------------------

# Runtime Architecture

Runtime does NOT generate textures.

Runtime consumes baked tiles and focuses on:

-   Placement
-   Subdivision
-   Transform generation
-   GPU rendering

------------------------------------------------------------------------

# Core Runtime Data Model

``` csharp
struct TileInstance
{
    public float4x4 transform;
    public int motifIndex;
    public int level;
}
```

Design rationale:

-   transform → world transform
-   motifIndex → index into TileSet
-   level → subdivision depth / parity / LOD

No per-instance texture references. GPU-friendly. Burst-compatible.

------------------------------------------------------------------------

# Placement Modes

## 1) Regular Grid Mode

-   Fixed cell size
-   No subdivision
-   Uniform transform scale
-   Ideal for deterministic tiling
-   Complexity: O(n)

## 2) Hierarchical QuadTree Mode

True tree, not flat list.

``` csharp
struct QuadNode
{
    public float2 position;
    public float size;
    public int level;
    public bool isLeaf;
    public int childIndex;
}
```

Characteristics:

-   Depth-based subdivision
-   Probabilistic subdivision
-   View-dependent subdivision (future)
-   LOD-aware rendering

Worst-case: O(4\^D)\
Practical: O(n) where n = number of leaf tiles

------------------------------------------------------------------------

# Winged Mode Support

Winged tiles are:

-   Baked at editor time
-   Texture-based at runtime
-   Compatible with both Regular Grid and QuadTree modes

Multiscale effect:

scale = 1 / (2\^level)

Tile cooker supports winged motifs via Bezier-based command DSL. Runtime
supports winged mode by respecting over-boundary motif geometry and
level scaling.

------------------------------------------------------------------------

# GPU Rendering Strategy

Primary target:

Graphics.DrawMeshInstancedIndirect

Per-instance data:

-   Transform matrix
-   Motif index
-   Level

Motif storage options:

A)  Single quad mesh + procedural shader (preferred)\
B)  Texture array atlas

------------------------------------------------------------------------

# Optimization Roadmap

## Rendering

-   Switch fully to indirect instancing
-   StructuredBuffer for TileInstance
-   Frustum culling per QuadNode
-   Shader-based motif logic

## Memory

-   Compress transform to float3x2
-   Half precision where possible
-   NativeList usage

## Subdivision

-   Burst job-based QuadTree
-   View-dependent subdivision
-   Deterministic per-node seeds

## Shader

-   Branchless parity logic
-   SDF-based winged motifs
-   Animated motif support

------------------------------------------------------------------------

# Design Philosophy

1.  Store transforms, not geometry.
2.  Bake visuals, not logic.
3.  Separate placement from rendering.
4.  Push work to GPU.
5.  Keep CPU memory linear.
6.  Avoid per-frame allocations.
7.  Design for 100k+ tiles.

