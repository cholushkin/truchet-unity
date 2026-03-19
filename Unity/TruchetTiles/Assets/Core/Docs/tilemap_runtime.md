# TileMapRuntime — Execution Pipeline

## Overview

TileMapRuntime is the main entry point of the system.

It orchestrates:

Layout → Modifiers → Composition → Rendering

This is the ONLY place where the full pipeline is executed.

---

# Responsibilities

TileMapRuntime is responsible for:

- Creating layout (Grid or QuadTree)
- Collecting TileSets from modifiers
- Applying modifiers
- Running composition strategy
- Passing result to rendering backend

It does NOT:

- Perform rendering logic itself
- Generate textures
- Contain composition logic

---

# Execution Flow

## 1. Layout Creation

Depending on mode:

- RegularGridTileMap
- QuadTreeTileMap

---

## 2. Modifier Collection

All TileMapModifier components are collected.

Each modifier:
- Owns a TileSet
- Receives TileSetId
- Mutates layout

---

## 3. Apply Modifiers

Modifiers are applied in order:

modifier.Apply(layout)

They define tile content.

---

## 4. GPU Resource Build

TileSets → Texture2DArray via:

TileSetGPUResourceManager

Result:
- Texture array
- TileSet offsets

---

## 5. Composition

Layout + TileSets → ICompositionResult

Current:

MotifInstanceCompositionStrategy

Output:

InstanceCompositionResult

---

## 6. Rendering

Each frame:

RenderBackend.RenderInstances(...)

Backend handles:
- GPU buffers
- Draw calls

---

# Frame Loop

Start():
- Initialize pipeline
- Generate layout
- Build resources
- Compose once

Update():
- Render every frame

---

# Layout Modes

## Regular Grid

- Fixed resolution
- Simple iteration

## QuadTree

- Hierarchical
- Adaptive subdivision

---

# Important Notes

- Rendering is continuous (per frame)
- Composition is currently done once (Start)
- Future: dynamic regeneration support

---

# Design Principles

- Single orchestration point
- Clean separation of concerns
- Replaceable composition strategies
- Replaceable rendering backends

---

# Summary

TileMapRuntime connects all systems together.

It defines WHEN things happen,
not HOW they are implemented.
