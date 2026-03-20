# Truchet Core --- GPU-Driven Procedural Tile System

## Overview

Truchet Core is a modular system for procedural tile-based rendering
designed for scalability, flexibility, and performance.

Core pipeline:

Layout → Composition → Rendering

Each layer has a single responsibility and can be replaced
independently.

------------------------------------------------------------------------

## Architecture

### Layout Layer

Defines spatial structure and tile placement.

Implementations: - RegularGrid - QuadTree

Responsibilities: - Spatial indexing - Tile storage - Structural
transformations

------------------------------------------------------------------------

### Composition Layer

Transforms layout data into renderer-ready data.

Interface: ICompositionStrategy

Current implementation: - InstanceComposition

------------------------------------------------------------------------

### Rendering Layer

Responsible for drawing data produced by composition.

Interface: IRenderBackend

Current implementation: - GPUInstancedRenderBackend

------------------------------------------------------------------------

## GPU Rendering

-   Uses GPU instancing for large-scale rendering
-   Texture2DArray for efficient texture binding
-   Indirect draw calls for performance

------------------------------------------------------------------------

## Extensibility

The system is designed to support: - Additional layout types -
Alternative composition strategies - Multiple rendering backends

------------------------------------------------------------------------

## Summary

Truchet Core provides a clean, scalable architecture for procedural tile
rendering with strong separation of concerns.
