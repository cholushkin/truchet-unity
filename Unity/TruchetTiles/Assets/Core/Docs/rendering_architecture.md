# Rendering Architecture

## Overview

The Rendering layer is responsible for drawing composition results.

It does not modify layout or perform composition.

------------------------------------------------------------------------

## Interface

IRenderBackend

------------------------------------------------------------------------

## Responsibilities

-   Receive composition results
-   Upload data to GPU
-   Execute draw calls

------------------------------------------------------------------------

## Current Backend

GPUInstancedRenderBackend

Pipeline: Instance data → GPU buffers → DrawMeshInstancedIndirect

------------------------------------------------------------------------

## Features

-   High-performance instanced rendering
-   Persistent GPU buffers
-   Texture2DArray sampling

------------------------------------------------------------------------

## Design Principles

-   Stateless rendering per frame
-   No knowledge of layout
-   No composition logic

------------------------------------------------------------------------

## Summary

Rendering is fully decoupled and focused purely on drawing.
