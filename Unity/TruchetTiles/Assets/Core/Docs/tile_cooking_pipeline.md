# Tile Cooking Pipeline — Truchet Core

## Overview

Tile Cooking is a procedural system that generates tile textures
from instruction scripts.

It replaces manual texture creation with deterministic, script-driven generation.

---

# Pipeline

TileCookDefinition
   ↓
Command Script
   ↓
TileCommandParser
   ↓
Render Instructions
   ↓
Rasterizer
   ↓
Texture2D (PNG)
   ↓
Tile Asset

---

# Core Components

## TileCookDefinition

Defines:
- Output resolution
- Winged mode
- Output folder
- Command script

---

## TileCommandParser

Parses text script into render instructions.

Supported primitives:
- Rectangle (RCT)
- Ellipse (ELP)
- Pie (PIE)
- Bezier (BZR)

Supports optional color prefix:
- B = Black
- W = White

---

## Rasterizer

Executes instructions and fills pixel buffer.

Features:
- Deterministic output
- Logical coordinate system (0..1)
- Winged tile support
- Multiple primitive rendering

---

# Winged Mode

When enabled:

- Texture resolution is doubled
- Logical tile occupies center (50%)
- Outer region is transparent

Used for:
- Overlapping tiles
- Multiscale rendering
- Future LOD systems

---

# Coordinate System

All instructions operate in normalized space:

(0,0) → bottom-left  
(1,1) → top-right  

This ensures resolution-independent definitions.

---

# Output

Each cook produces:

- PNG texture
- Tile ScriptableObject

Tile contains:
- Texture reference
- Winged flag

---

# Example Script

```
RCT 0.5 0.5 1.0 1.0
BZR 0 0 1 1 0 1 1 0 0.1
```

---

# Design Goals

- Fully procedural tiles
- Deterministic generation
- No manual art pipeline required
- Easy iteration

---

# Limitations (Current)

- No anti-aliasing
- Fixed color palette (black/white)
- CPU-based rasterization

---

# Future Improvements

- Anti-aliasing / supersampling
- Color support
- GPU-based cooking
- Atlas generation

---

# Summary

Tile Cooking enables fast, deterministic generation of tile textures
using simple scripts, forming the foundation of the rendering pipeline.
