# Truchet System: Architecture Overview

## Design Principles
- **Unidirectional Data Flow:** Layout -> Composition -> Rendering.
- **Dependency Injection Ready:** Services are decoupled and interface-driven, preparing the system for VContainer integration.
- **Topology Agnostic:** The core data structures do not care if the world is made of squares or triangles.

---

## 1. Core State & Data (Model)

### `HierarchicalTree`
The pure data container. Holds the 1-to-4 node array and handles memory pooling (FreeBlocks).
- **Responsibilities:** Allocation, subdivision logic, collapse logic, and state serialization.
- **Contains:** `Node[]`, `Stack<int> FreeBlocks`.

### `ITopologyStrategy`
Defines the spatial reality of the tree (Square vs. Triangle).
- **Implementations:** `SquareTopology`, `TriangleTopology`.
- **Responsibilities:** Translates logical bounds to world space, resolves `FindLeafAt(UV)`, and defines rotational constraints (90° vs 120°).

---

## 2. Pipeline Services (Broker/Controller)

### `LayoutGenerator`
Handles the deterministic generation of the initial state.
- **Responsibilities:** Initializes RNG, applies `LayoutModifier` components in a strict priority order, and guarantees reproducible results.

### `CompositionService`
The bridge between abstract nodes and renderable data.
- **Responsibilities:** Traverses the `HierarchicalTree`, applies topology-specific transforms, resolves winged margins, and outputs an array of `TileInstanceGPU`.

### `InteractionService`
The sole owner of volatile, user-driven state changes.
- **Responsibilities:** Translates user input (Mode, UV) into spatial queries via `ITopologyStrategy`, mutates the `HierarchicalTree`, and requests a composition rebuild.

---

## 3. Rendering (View)

### `IRenderBackend`
Consumes `TileInstanceGPU` arrays.
- **Implementations:** `GPUInstancedRenderBackend`, `TextureRenderBackend`.
- **Responsibilities:** Buffer management, draw calls, and batching. Knows nothing about the tree structure.

---

## 4. Authoring (Tooling)

### `TileCooker`
Procedural rasterization pipeline.
- **Responsibilities:** Parses text instructions (`RCT`, `BZR`) into pixels.
- **Extensions:** Uses `ICookGeometryStrategy` to handle square bounds or triangular bounds (with respective wing overlaps).