# Truchet Tiles for Unity (WIP)

A multi-scale Truchet tile pattern generator ported from a C# CLI
implementation to Unity.

This project is based on the original multi-scale Truchet pattern
generator described by Christopher Carlson and previously implemented
using `System.Drawing`.

This Unity port replaces raster GDI rendering with a modular,
engine-native architecture built for:

-   Deterministic procedural generation
-   Multi-scale hierarchical subdivision
-   Strategy-driven tile selection
-   Future GPU / Mesh / Shader rendering
-   Extensibility and experimentation

⚠️ **Status: Work In Progress** Core generation logic is functional.
Rendering pipeline and palette system are partially implemented.

------------------------------------------------------------------------

# Architecture Overview

The project is structured into clean domain layers:

## Core (Pure C#)

-   `TileType` + bitmask connectivity
-   `TileTypeExtensions`
-   `GenerationSettings`
-   `ITileSelectionStrategy`
-   `RandomTileSelectionStrategy`
-   `PerlinTileSelectionStrategy` (Unity-based fBm)
-   `TileNode` (hierarchical structure)
-   `TileTreeBuilder`
-   `TileDrawCommand`
-   `Tileset` (geometry command emitter)

## Unity Layer

-   `GenerationSettingsScriptableObject`
-   `TileTreeDebugVisualizer`
-   `ITileTreeRenderer` (abstraction only, no backend yet)

## Legacy (Original GDI Implementation)

Located under `truchlib~`. Kept temporarily for reference during
porting.

------------------------------------------------------------------------

# Current Capabilities

-   Deterministic grid generation
-   Deterministic multi-scale subdivision
-   Perlin-based and random tile selection
-   ScriptableObject-based configuration
-   Scene-view Gizmo debug visualization
-   Hierarchical tree debug printing

------------------------------------------------------------------------

# Roadmap

------------------------------------------------------------------------

## Foundation Phase

-   TODO: Remove all remaining legacy GDI code (`truchlib~`) completely.
-   TODO: Implement Unity-native palette system (ScriptableObject
    based).
-   TODO: Implement Texture2D tile renderer backend using
    `TileDrawCommand`.
-   TODO: Implement proper coordinate-to-world mapping for large grids.
-   TODO: Add validation and guardrails to `GenerationSettings`.
-   TODO: Add editor-time regeneration controls (Play Mode toggle,
    manual refresh).
-   TODO: Add performance-safe rebuild mechanism to avoid excessive
    `OnValidate` calls.
-   TODO: Add unit tests for deterministic generation.

------------------------------------------------------------------------

## Extension Phase

-   TODO: Implement adjacency constraint-aware tile selection.
-   TODO: Add weighted tile distribution system.
-   TODO: Add AnimationCurve remapping for Perlin noise.
-   TODO: Add probabilistic subdivision control per level.
-   TODO: Add runtime streaming / chunk-based generation.
-   TODO: Add mesh-based tile renderer backend.
-   TODO: Add GPU instancing for large grids.
-   TODO: Implement ScriptableObject-based tile metadata system.
-   TODO: Add LOD-based hierarchical rendering system.
-   TODO: Add custom inspector for generation presets.
-   TODO: Add export-to-texture functionality (PNG output).
-   TODO: Add border cropping / padding support (parity with original
    CLI).

------------------------------------------------------------------------

## Advanced Phase

-   TODO: Implement shader-based procedural tile rendering (no
    textures).
-   TODO: Add animated subdivision transitions.
-   TODO: Implement Wave Function Collapse variant using TileType
    connectivity.
-   TODO: Add runtime biome blending (multiple strategies per region).
-   TODO: Add domain warping to Perlin selection strategy.
-   TODO: Make generation Burst-compatible + Job System compatible.
-   TODO: Add GPU compute shader noise backend.
-   TODO: Add dynamic tile rotation and symmetry system.
-   TODO: Add adaptive subdivision based on camera distance.
-   TODO: Add infinite scrolling procedural world mode.
-   TODO: Add addressables integration for tile sets and palettes.

------------------------------------------------------------------------

## Future / Experimental Ideas

-   TODO: Real-time audio-reactive subdivision patterns.
-   TODO: VR procedural pattern environment.
-   TODO: 3D volumetric Truchet cubes.
-   TODO: Physics-driven subdivision triggers.
-   TODO: Neural-network-driven tile selection strategies.
-   TODO: Interactive runtime pattern painting tool.
-   TODO: Multiplayer synchronized generative art mode.
-   TODO: Procedural architecture generator using Truchet connectivity.
-   TODO: Generative NFT batch exporter with deterministic seeds.
-   TODO: Generative city grid using tile connectivity as road network.

------------------------------------------------------------------------

# Design Principles

-   Deterministic generation
-   Modular architecture
-   Clear separation of domain and engine layers
-   No hidden coupling
-   Extensible strategy-based design
-   Future GPU / performance scalability
-   Editor-friendly workflow

------------------------------------------------------------------------

# Known Limitations

-   No production rendering backend yet (debug-only visualization).
-   Palette system not yet ported to Unity-native color system.
-   Some legacy files still present for reference.
-   No adjacency constraints yet.
-   No streaming optimization yet.

------------------------------------------------------------------------

# Final Goal

A fully modular, extensible, high-performance, multi-scale Truchet
pattern system for Unity that supports:

-   Deterministic generation
-   Hierarchical subdivision
-   Strategy-based tile selection
-   Multiple rendering backends
-   Runtime animation
-   Infinite procedural worlds
-   GPU acceleration

------------------------------------------------------------------------

# License

(Define license here.)
