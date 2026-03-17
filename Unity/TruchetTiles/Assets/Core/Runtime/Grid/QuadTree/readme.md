# QuadTreeTileMap --- Architecture & Design Specification

## Overview

QuadTreeTileMap is a mutable hierarchical spatial container supporting
both:

-   Uniform grid access (Mode A)
-   Adaptive hierarchical access (Mode B)

It is designed to support: - Deterministic procedural generation -
Runtime structural mutation (subdivide / collapse) - Designer patch
workflows - Modifier-driven content changes - Future winged multiscale
rendering

The TileMap is strictly a container. It does not generate content
automatically. All generation logic is handled by modifiers.

------------------------------------------------------------------------

# Core Architectural Principles

1.  Stable Node Identity\
2.  Deterministic Structural Behavior\
3.  Canonical Child Ordering\
4.  Strict Mode Separation\
5.  No Hidden Side Effects\
6.  Spatial Traversal over Morton Encoding\
7.  Modifiers Own Content Generation

------------------------------------------------------------------------

# Interfaces

QuadTreeTileMap implements:

-   IGridLayout (Mode A only)
-   IHierarchicalTileLayout (always available)

------------------------------------------------------------------------

# Modes

## Mode A --- Uniform Depth (Grid Compatible)

Valid only if:

All active leaves share the same Level.

When valid:

-   Width = Height = 2\^depth
-   Spatial traversal resolves (x, y)
-   IGridLayout is fully supported

If not uniform:

-   IGridLayout methods throw InvalidOperationException

------------------------------------------------------------------------

## Mode B --- Adaptive Hierarchical Mode

-   Leaves may exist at mixed levels
-   IGridLayout is invalid
-   IHierarchicalTileLayout remains fully usable

------------------------------------------------------------------------

# Canonical Child Ordering

Child indices are permanently defined as:

```
  2   3
  --- ---
  0   1
```

0 = bottom-left\
1 = bottom-right\
2 = top-left\
3 = top-right

This ordering must never change.

Collapse behavior depends on this ordering.

------------------------------------------------------------------------

# Node Structure

Each node contains:

-   X, Y (spatial origin)
-   Size
-   Level
-   IsLeaf
-   IsActive
-   ChildIndex
-   TileSetId
-   TileIndex
-   Rotation

Inactive nodes retain their data.

Nodes are never physically removed from memory.

------------------------------------------------------------------------

# Stable Node Indices

-   Node indices never shift.
-   Collapse marks children inactive.
-   Subdivision may reuse indices from a free-list.
-   External systems (patch, modifiers) may safely store node indices.

------------------------------------------------------------------------

# Subdivision Rules

When Subdivide(node):

-   Only valid if node is a leaf.
-   4 children are created.
-   Children inherit parent tile values.
-   Parent becomes non-leaf.
-   Uniform state is recalculated.

------------------------------------------------------------------------

# Collapse Rules

When Collapse(node):

-   Only valid if node is non-leaf.
-   Parent tile becomes child\[0\] tile.
-   Children are marked inactive.
-   Children retain stored tile data.
-   Uniform state is recalculated.

No automatic merging heuristics exist.

------------------------------------------------------------------------

# Uniform Detection

Uniform mode is valid when:

All active leaves have identical Level.

This is recalculated after every structural mutation.

------------------------------------------------------------------------

# Spatial Traversal

Grid access uses spatial traversal:

-   Start at root
-   Descend by quadrant selection
-   Resolve leaf in O(depth)

Morton indexing is intentionally NOT used.

Reason:

-   Preserves structural semantics
-   Works for adaptive trees
-   Safer for mutable systems
-   Easier for patch workflows

------------------------------------------------------------------------

# Determinism

-   QuadTreeTileMap does not auto-generate randomness.

-   If random subdivision is implemented, it must use:

    nodeSeed = Hash(globalSeed, node.Level, node.X, node.Y)

-   No UnityEngine.Random allowed.

Tree structure changes only through explicit calls.

------------------------------------------------------------------------

# Mutability

Allowed operations:

-   Subdivide(node)
-   Collapse(node)
-   SetTileByNode(node)
-   SetTile(x,y) (uniform mode only)

Structure never rebuilds implicitly.

------------------------------------------------------------------------

# Modifier Responsibility

Modifiers:

-   Own procedural generation
-   Own random behavior (seeded)
-   Mutate tile content
-   Do not own structure unless explicitly intended

TileMap is a passive container.

------------------------------------------------------------------------

# Rendering Implications

Grid renderer operates in Mode A.

Future winged multiscale renderer will use:

-   IHierarchicalTileLayout
-   Node spatial data
-   Level-based scaling

------------------------------------------------------------------------

# Non-Goals

QuadTreeTileMap will NOT:

-   Auto-balance structure
-   Auto-generate content
-   Implicitly rebuild on seed change
-   Hide invalid grid access in adaptive mode

------------------------------------------------------------------------

# Future Extensions

-   Adaptive view-dependent LOD
-   Burst-compatible node storage
-   GPU transform streaming
-   Constraint-based subdivision modifiers

------------------------------------------------------------------------

# Summary

QuadTreeTileMap is:

-   Deterministic
-   Mutable
-   Designer-safe
-   Dual-interface compliant
-   Strictly structured
-   Future-proof for multiscale winged rendering
