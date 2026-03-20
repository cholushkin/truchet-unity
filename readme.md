![logo](Documentation/repository-open-graph-cover.jpg)

# Truchet Core

![Unity](https://img.shields.io/badge/Engine-Unity-black?logo=unity)
![Rendering](https://img.shields.io/badge/Rendering-GPU%20Instanced-blue)
![Architecture](https://img.shields.io/badge/Architecture-Modular-green)
![Status](https://img.shields.io/badge/Status-Active-success)

Procedural GPU-driven tile system for Unity focused on scalability,
performance, and clean architecture.

------------------------------------------------------------------------

## 🎬 Preview

> *(Add GIF or screenshot here)*\
> Example:

    ![demo](Documentation/demo.gif)

------------------------------------------------------------------------

## 🚀 Features

-   ⚡ GPU instanced rendering (100k+ tiles)
-   🌳 QuadTree hierarchical layouts
-   🎨 Multi-tileset support
-   🧩 Texture2DArray batching
-   🛠 Procedural tile generation (Tile Cooking)
-   🧱 Modular architecture (replace any layer)

------------------------------------------------------------------------

## 🧠 Architecture

    Layout → Composition → Rendering

  Layer         Responsibility
  ------------- ------------------------------------
  Layout        Spatial structure & tile placement
  Composition   Convert layout → renderable data
  Rendering     GPU drawing & execution

------------------------------------------------------------------------

## 📦 Documentation

### Core

-   [Core README](./Unity/TruchetTiles/Assets/Core/Docs/readme.md)

### Systems

-   [QuadTree](./Unity/TruchetTiles/Assets/Core/Docs/quadtree.md)
-   [Rendering
    Architecture](./Unity/TruchetTiles/Assets/Core/Docs/rendering_architecture.md)
-   [Composition
    Layer](./Unity/TruchetTiles/Assets/Core/Docs/composition_layer.md)
-   [Texture Array
    System](./Unity/TruchetTiles/Assets/Core/Docs/texture_array_system.md)
-   [TilePipeline
    Runtime](./Unity/TruchetTiles/Assets/Core/Docs/tilemap_runtime.md)
-   [Tile Cooking
    Pipeline](./Unity/TruchetTiles/Assets/Core/Docs/tile_cooking_pipeline.md)

------------------------------------------------------------------------

## ⚡ Quick Start

1.  Open Unity project\
2.  Add **TilePipeline** to a GameObject\
3.  Assign:
    -   Material
    -   TileSets (via modifiers)
4.  Press Play 🚀

------------------------------------------------------------------------

## 🏗 Key Concepts

### Layout

Defines where tiles exist.

### Modifiers

Populate layout with tile data.

### Composition

Builds renderable representation.

### Rendering

Draws everything using GPU instancing.

------------------------------------------------------------------------

## 📌 Entry Point

Start here:

👉 `TilePipeline`

------------------------------------------------------------------------

## 🧩 Design Goals

-   Clean separation of concerns\
-   High performance (GPU-first)\
-   Fully replaceable systems\
-   Deterministic behavior

------------------------------------------------------------------------

## 📜 License

*(Add your license here)*

------------------------------------------------------------------------

## ⭐ Notes

This project is designed as a **foundation for procedural worlds**, not
just a tile renderer.

You can extend it with: - Custom layouts - New composition strategies -
Alternative rendering backends
