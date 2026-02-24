# Multi-Scale Truchet Tiles Rendering System in Unity

## Implementation Plan

------------------------------------------------------------------------

# 1. System Goals

Build an efficient, scalable rendering system for multi-scale (winged)
Truchet tiles in Unity with:

-   Efficient GPU-driven rendering
-   True hierarchical QuadTree (not flat list)
-   Efficient subdivision logic
-   Clean data-oriented structures
-   Designed for large tile counts
-   Optimization roadmap included

------------------------------------------------------------------------

# 2. Architecture Overview

System consists of:

1.  **QuadTree Subdivision System**
2.  **Tile Data Model**
3.  **Transform Generation**
4.  **Parity & Scale Computation**
5.  **Rendering System (GPU Instancing)**
6.  **Optional Compute/Jobs Acceleration**

------------------------------------------------------------------------

# 3. Data Model

## 3.1 QuadTree Node (Hierarchical)

Each node represents a square region.

``` csharp
struct QuadNode
{
    public float2 position;      // bottom-left corner
    public float size;           // square side length
    public int level;            // depth in tree

    public bool isLeaf;

    public int childIndex;       // index into node array (first of 4 children)
}
```

Store nodes in a **NativeList`<QuadNode>`{=html}** for cache-friendly
traversal.

Children are stored consecutively: childIndex, childIndex+1, +2, +3

------------------------------------------------------------------------

## 3.2 Tile Instance Data

Only leaf nodes produce renderable tiles.

``` csharp
struct TileInstance
{
    public float4x4 transform;
    public int motifIndex;
    public int level;
}
```

Avoid storing scale explicitly. Scale = 1 / (2\^level)

------------------------------------------------------------------------

# 4. Efficient Subdivision Algorithm

## 4.1 Iterative Stack-Based Subdivision

Avoid recursion.

``` csharp
Stack<int> stack;
stack.Push(rootIndex);

while(stack.Count > 0)
{
    int index = stack.Pop();
    QuadNode node = nodes[index];

    if(ShouldStop(node))
    {
        node.isLeaf = true;
        EmitTile(node);
        continue;
    }

    Subdivide(node);
}
```

------------------------------------------------------------------------

## 4.2 Stop Conditions

### Fixed depth

    if(node.level >= maxDepth)

### Probabilistic subdivision

    float p = baseProbability * (1f - node.level/maxDepth);
    if(Random.value > p)

### Region Fill (Efficient)

Test square corners only.

-   All inside → accept
-   All outside → discard
-   Mixed → subdivide

No geometry intersection math.

------------------------------------------------------------------------

# 5. Transform Generation

For each leaf node:

``` csharp
float scale = 1f / (1 << node.level);

Matrix4x4 T =
    Matrix4x4.TRS(
        new Vector3(node.position.x, node.position.y, 0),
        Quaternion.identity,
        new Vector3(scale, scale, 1)
    );
```

------------------------------------------------------------------------

# 6. Parity Calculation (Color Inversion)

Parity = level % 2

``` csharp
bool invert = (node.level & 1) == 1;
```

Send invert flag to shader as instance property.

------------------------------------------------------------------------

# 7. Efficient Rendering Strategy

## 7.1 Use GPU Instancing

Use: - Graphics.DrawMeshInstancedIndirect -
StructuredBuffer`<TileInstance>`{=html}

Per-instance data: - transform matrix - motif index - parity flag

------------------------------------------------------------------------

## 7.2 Motif Storage

Options:

### A) Single Mesh + Shader Logic

Pass motifIndex → shader draws arcs procedurally

### B) Mesh per Motif

Store mesh library Batch by motifIndex

Preferred: **Single mesh + procedural shader** Most scalable.

------------------------------------------------------------------------

# 8. Real QuadTree (Not Flat)

Traversal options:

## 8.1 Depth-First Rendering (optional future)

Allows progressive refinement.

## 8.2 Level Buckets (for ordering)

Maintain:

Dictionary\<int, NativeList`<int>`{=html}\> levelBuckets;

Render from lowest level to highest. Large tiles first → small tiles on
top.

------------------------------------------------------------------------

# 9. Efficient Data Structures

Use:

-   NativeList`<QuadNode>`{=html}
-   NativeList`<TileInstance>`{=html}
-   NativeArray for GPU upload
-   Burst + Jobs for subdivision

Avoid:

-   Managed Lists in update loop
-   Recursive allocation
-   Per-frame memory churn

------------------------------------------------------------------------

# 10. Efficient Subdivision (Parallel Version)

Use IJobParallelFor for splitting large node sets.

Pipeline:

1.  Collect nodes to subdivide
2.  Parallel create children
3.  Append to NativeList with ParallelWriter

------------------------------------------------------------------------

# 11. Optimization Roadmap (TODO)

## Rendering

-   [ ] Switch to DrawMeshInstancedIndirect
-   [ ] Frustum culling per QuadTree node
-   [ ] Hierarchical LOD (stop subdividing distant nodes)
-   [ ] ComputeShader-based tile generation
-   [ ] Indirect draw argument buffer reuse

## Memory

-   [ ] Object pooling for QuadNodes
-   [ ] Compress transform to float3x2 (2D affine)
-   [ ] Use half precision where possible

## Subdivision

-   [ ] View-dependent adaptive subdivision
-   [ ] Cached region mask texture for fast inside tests
-   [ ] Precomputed random seeds per node (deterministic patterns)

## Shader

-   [ ] Branchless parity inversion
-   [ ] Procedural arc SDF generation
-   [ ] Merge motif logic into single SDF system

------------------------------------------------------------------------

# 12. Advanced Future Extensions

-   Compute-driven fully GPU QuadTree
-   Infinite tiling via clipmap system
-   3D winged tiles
-   Animation via time-dependent motif transforms
-   Hybrid triangular + square tiling

------------------------------------------------------------------------

# 13. Development Phases

Phase 1 -- Basic System - QuadTree generation - CPU instanced rendering

Phase 2 -- GPU Optimized - Indirect instancing - Burst subdivision

Phase 3 -- Advanced Optimization - LOD - Frustum culling - Compute
pipeline

Phase 4 -- Infinite Scalable Renderer - Streaming tiles -
Camera-dependent subdivision

------------------------------------------------------------------------

# 14. Expected Complexity

Worst case: O(4\^D)

Practical: O(n) where n = number of leaf tiles

Memory: O(n)

Rendering: O(n) instanced draw cost

------------------------------------------------------------------------

# 15. Final Notes

Key design philosophy:

-   Store transforms, not geometry
-   Use hierarchy, not flat arrays
-   Sort by level, not scale
-   Push work to GPU
-   Keep CPU memory linear and cache-friendly
