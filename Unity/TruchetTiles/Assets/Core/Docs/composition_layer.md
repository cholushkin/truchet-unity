# Composition Layer

## Overview

The Composition layer converts layout data into renderer-ready results.

It acts as a bridge between layout and rendering.

------------------------------------------------------------------------

## Interface

ICompositionStrategy

Compose(layout, tileSets, resolution)

------------------------------------------------------------------------

## Responsibilities

-   Read layout data
-   Generate renderable data structures
-   Remain independent of rendering APIs

------------------------------------------------------------------------

## Output

All composition results implement:

ICompositionResult

Current type: - InstanceCompositionResult

------------------------------------------------------------------------

## Current Implementation

InstanceComposition

Uses: - GridInstanceBuilder - QuadTreeInstanceBuilder

------------------------------------------------------------------------

## Design Goals

-   Renderer-agnostic
-   Replaceable strategies
-   Clean separation from layout and rendering
