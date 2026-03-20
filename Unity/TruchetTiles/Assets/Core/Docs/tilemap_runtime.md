# TilePipeline Runtime

## Overview

TilePipeline orchestrates the full execution pipeline.

Layout → Modifiers → Composition → Rendering

------------------------------------------------------------------------

## Responsibilities

-   Create layout
-   Apply modifiers
-   Build GPU resources
-   Execute composition
-   Trigger rendering

------------------------------------------------------------------------

## Execution Flow

1.  Layout creation
2.  Modifier application
3.  Resource preparation
4.  Composition
5.  Rendering

------------------------------------------------------------------------

## Layout Modes

-   RegularGrid
-   QuadTree

------------------------------------------------------------------------

## Runtime Behavior

-   Composition runs during initialization
-   Rendering runs every frame

------------------------------------------------------------------------

## Summary

TilePipeline defines when each system runs and connects all components
together.
