# QuadTreeTileMap — Architecture & Design Specification (UPDATED)

## Overview

QuadTreeTileMap is a hierarchical spatial container supporting:

- Uniform grid mode (fixed depth)
- Adaptive hierarchical mode (mixed depth)

It is designed for:

- Deterministic procedural generation
- Runtime structural mutation (subdivide / collapse)
- Modifier-driven content workflows
- GPU-friendly instance generation

IMPORTANT:

QuadTreeTileMap is a **pure topology container**.

It:
- DOES NOT render
- DOES NOT generate textures
- DOES NOT know about GPU
- DOES NOT contain connectivity logic

---

# Core Principles

1. Stable Node Identity  
2. Deterministic Behavior  
3. Canonical Child Ordering  
4. No Hidden Side Effects  
5. Layout is Passive  
6. Modifiers Own Content  

---

# Interfaces

QuadTreeTileMap implements:

- IGridLayout (only in uniform mode)
- IHierarchicalTileLayout (always)

---

# Modes

## Mode A — Uniform Grid Mode

Valid when all leaves share same depth.

Provides:
- Width / Height
- Grid access (x, y)

## Mode B — Adaptive Mode

- Mixed depth leaves
- Grid interface disabled
- Use hierarchical traversal only

---

# Node Structure

Each node contains:

- Position (X, Y)
- Size
- Level
- TileSetId
- TileIndex
- Rotation

No topology / connectivity data is stored.

---

# Subdivision

Subdivide(node):
- Creates 4 children
- Children inherit tile data
- Parent becomes non-leaf

---

# Collapse

Collapse(node):
- Parent takes child[0] tile
- Children become inactive
- Data is preserved

---

# Determinism

- No internal randomness
- Modifiers must be deterministic
- No implicit rebuilds

---

# Rendering Relationship

QuadTreeTileMap does NOT render anything.

Instead:

Layout → Composition → Rendering

Example:

QuadTree → InstanceComposition → GPUInstancedRenderBackend

---

# Summary

QuadTreeTileMap is:

- Pure topology
- Deterministic
- Mutable
- Rendering-agnostic
