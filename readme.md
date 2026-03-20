![logo](Documentation/repository-open-graph-cover.jpg)

# Truchet Core

Procedural GPU-driven tile system for Unity.

------------------------------------------------------------------------

## 📦 Documentation Overview

### 🔹 Core Review

-   **Readme:** [Core
    README](./Unity/TruchetTiles/Assets/Core/Docs/readme.md)
-   **Description:** High-level architecture overview of the system,
    including layout, composition, and rendering separation.

------------------------------------------------------------------------

### 🔹 QuadTree

-   **Readme:**
    [QuadTree](./Unity/TruchetTiles/Assets/Core/Docs/quadtree.md)
-   **Description:** Hierarchical data structure, deterministic
    subdivision, and spatial traversal model.

------------------------------------------------------------------------

### 🔹 Rendering Architecture

-   **Readme:** [Rendering
    Architecture](./Unity/TruchetTiles/Assets/Core/Docs/rendering_architecture.md)
-   **Description:** GPU instancing pipeline, rendering backends, and
    draw call orchestration.

------------------------------------------------------------------------

### 🔹 Composition Layer

-   **Readme:** [Composition
    Layer](./Unity/TruchetTiles/Assets/Core/Docs/composition_layer.md)
-   **Description:** Transformation from layout data into
    renderer-agnostic results.

------------------------------------------------------------------------

### 🔹 Texture Array System

-   **Readme:** [Texture
    Array](./Unity/TruchetTiles/Assets/Core/Docs/texture_array_system.md)
-   **Description:** Multi-tileset GPU resource building, caching, and
    motif indexing.

------------------------------------------------------------------------

### 🔹 TileMap Runtime

-   **Readme:** [TileMap
    Runtime](./Unity/TruchetTiles/Assets/Core/Docs/tilemap_runtime.md)
-   **Description:** Execution pipeline connecting layout, modifiers,
    composition, and rendering.

------------------------------------------------------------------------

### 🔹 Tile Cooking Pipeline

-   **Readme:** [Tile
    Cooking](./Unity/TruchetTiles/Assets/Core/Docs/tile_cooking_pipeline.md)
-   **Description:** Procedural tile texture generation from command
    scripts.

------------------------------------------------------------------------

## 🧠 Architecture Summary

    Layout → Composition → Rendering

-   **Layout:** Defines spatial structure\
-   **Composition:** Produces renderable data\
-   **Rendering:** Draws using GPU

------------------------------------------------------------------------

## 🚀 Key Features

-   GPU instanced rendering (100k+ tiles)
-   QuadTree hierarchical layouts
-   Multi-tileset support
-   Texture2DArray batching
-   Procedural tile generation
-   Clean modular architecture

------------------------------------------------------------------------

## 📌 Notes

-   All systems are decoupled and replaceable
-   Designed for performance and scalability
-   Focused on data-oriented architecture

------------------------------------------------------------------------

## 🔗 Entry Point

Start from:

👉 `TilePipeline`

This is where the full pipeline is executed.
