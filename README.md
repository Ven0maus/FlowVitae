# FlowVitae
FlowVitae is a memory and performance efficient 2D grid library designed for small to large scale procedural worlds.
Can be easily integrated with most render engines.

Tested with:
- SadConsole V9

# Features

**Different grid layouts**
- Static grid (no chunking)
- Procedural grid (chunking)

**Infinite chunking terrain**
- Chunking is automatically done
- Viewport and chunk size is configurable
- Method to center viewport on a coordinate
- Procedural generation algorithm can be passed straight to the Grid

**Easy to use**
- Possible to configure custom Grid, Cell, ProceduralGeneration classes
- Has a visualizer project that serves as an example on how to integrate with a render engine, such as SadConsole.
