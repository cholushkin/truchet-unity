# Composition Layer — Truchet Core

## Overview

The Composition layer transforms layout data into renderer-ready data.

It is the bridge between:

Layout → Rendering

---

# Core Interface

ITileCompositionStrategy

Method:

Compose(layout, tileSets, resolution) → ICompositionResult

---

# Key Principle

Composition produces data.

It does NOT:
- Render
- Access GPU APIs
- Modify layout

---

# Composition Results

All outputs implement:

ICompositionResult

Current types:

## 1. InstanceCompositionResult

Contains:
- List<TileInstanceGPU>
- Resolution

Used by:
GPUInstancedRenderBackend

---

## 2. MeshCompositionResult (planned)

Contains:
- Mesh
- Bounds
- Resolution

Used by:
Future mesh rendering

---

# Current Strategy

## MotifInstanceCompositionStrategy

Pipeline:

Layout → Instances

Supports:
- RegularGrid
- QuadTree
- Multi-tileset offsets

---

# Future Strategy

## MarchingSquaresCompositionStrategy

Pipeline:

Layout → Discrete Field → Mesh

Notes:
- No SDF
- Uses tile-derived field
- Produces mesh

---

# Design Goals

- Renderer-agnostic
- Replaceable strategies
- Supports multiple outputs
- Clean data separation

---

# Summary

Composition layer defines WHAT gets rendered,
not HOW it is rendered.
