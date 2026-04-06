# Marching Squares Backend — Truchet System

## Overview

The Marching Squares backend converts Truchet tiles into a continuous mesh by interpreting each tile as a small density field and combining them into a global field.

Unlike instanced rendering, this approach produces smooth, connected geometry across tile boundaries.

---

## Core Concept

Each tile is defined as a binary mask:

- Resolution: 16×16 (configurable)
- Values:
  - 0 = empty
  - 1 = filled

At runtime, all tile masks are merged into a single global field and processed with the Marching Squares algorithm.

---

## Full Pipeline

Layout  
→ Modifiers  
→ **TileInstances**  
→ Field Expansion  
→ Marching Squares  
→ Mesh  
→ Rendering

---

## Data Model

### Input

- `List<TileInstance>`

Each TileInstance contains:
- Position
- TileIndex
- Rotation

---

### Tile Mask Library

A lookup table:

```
Dictionary<int, bool[,]>
```

Maps:
```
TileIndex → 16×16 mask
```

Rotation is applied at runtime.

---

### Global Field

A dense 2D array:

```
float[,] or bool[,]
```

Size:
```
(gridWidth * tileResolution, gridHeight * tileResolution)
```

---

## Field Expansion

For each TileInstance:

1. Fetch tile mask
2. Apply rotation (0°, 90°, 180°, 270°)
3. Compute position in global field
4. Write mask into global field

Result:
- One continuous scalar field representing all tiles

---

## Marching Squares Step

For each cell in the field:

1. Sample 4 corners
2. Build case index (0–15)
3. Generate edges based on lookup table

Output:
- Vertices
- Indices

This produces a continuous mesh.

---

## Important Properties

### Connectivity

- Tiles merge seamlessly
- No visible seams between tiles

---

### Resolution Control

- Tile resolution defines mesh detail
- Higher resolution → smoother results, higher cost

---

### Deterministic

- Same layout → same mesh

---

## Backend Responsibilities

The Marching Squares backend:

- Stores tile masks
- Builds global field
- Runs marching squares
- Generates mesh

It does NOT:

- Modify layout
- Handle modifiers
- Perform composition

---

## Integration

The backend consumes:

```
List<TileInstance>
```

It acts as a rendering backend alternative to:

- GPU instancing
- Texture rasterization

---

## Comparison to Other Backends

| Backend              | Output        | Characteristics                  |
|---------------------|--------------|----------------------------------|
| GPU Instanced       | Quads        | Fast, discrete tiles             |
| Texture Rendering   | Texture2D    | CPU raster, simple               |
| Marching Squares    | Mesh         | Smooth, continuous geometry      |

---

## Future Extensions

- Variable tile resolution
- GPU marching squares
- Chunked processing
- LOD system
- Edge smoothing / interpolation

---

## Summary

The Marching Squares backend transforms discrete tile data into a continuous mesh by:

1. Expanding tiles into a global field  
2. Running marching squares on that field  
3. Generating a mesh from contours  

This enables smooth, connected rendering while keeping the tile-based workflow.
