# FlowVitae
FlowVitae is a memory and performance efficient 2D grid library designed for small to large scale procedural worlds.
Can be easily integrated with most render engines.

Supports:
- net6.0 
- net5.0
- netstandard2.1

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

# Setup
**FlowVitae grids use 2 generic types**
- TCellType is constrained to a struct, and will represent the unique cell value kept in memory. (eg, an int or byte that points to the real cell id)
- TCell is the real cell that represents the TCellType, it uses the ICell<TCellType> interface

**FlowVitae provides some basic implementations already out of the box.**
```csharp
Grid<TCellType, TCell>
Cell<TCellType>
```

**Static Grid Creation**
```csharp
var grid = new Grid<int, Cell<int>>(width, height);
```

**Procedural Grid Creation**
```csharp
var procGen = new ProceduralGenerator<int, Cell<int>>(Seed, GenerateChunkMethod);
var grid = new Grid<int, Cell<int>>(width, height, chunkWidth, chunkHeight, procGen);
```

GenerateChunkMethod can look something like this:
```csharp
public void GenerateChunkMethod(Random random, int[] chunk, int width, int height, (int x, int y) chunkCoordinate)
{
	// Every position contains default value of int (0) which could represent grass
	for (int x = 0; x < width; x++)
	{
		for (int y = 0; y < height; y++)
		{
			// Add a random chance for a cell to be a tree
			if (random.Next(0, 100) < 10)
				chunk[y * width + x] = 1;
		}
	}
}
```
The random already has a unique seed based on the provided Seed in the ProceduralGenerator and the chunk coordinate.
int[] chunk represent the chunk, int[] will be your TCellType[]
and the chunkCoordinate is provided too, in case you want to sample noise based on coordinates.

Chunks are generated automatically and they will use this method as reference to build the chunk.

# Setting custom chunk data	
It is possible to set custom data, per chunk which can be directly retrieved from the grid.
This custom data, can be any class that implements IChunkData interface. An example implementation:
```csharp
internal class TestChunkData : IChunkData
{
	public int Seed { get; set; }
	public List<(int x, int y)>? Trees { get; set; }
}
// Custom chunk generation implementation
Func<Random, int[], int, int, TestChunkData> chunkGenerationMethod = (random, chunk, width, height, chunkCoordinate) =>
{
	// Define custom chunk data
	var chunkData = new TestChunkData
	{
		Trees = new List<(int x, int y)>()
	};
	for (int x = 0; x < width; x++)
	{
		for (int y = 0; y < height; y++)
		{
			chunk[y * width + x] = random.Next(-10, 10);
			// Every 0 value is a tree, lets keep it for easy pathfinding access
			if (chunk[y * width + x] == 0)
				chunkData.Trees.Add((x, y));
		}
	}
	return chunkData;
};

// Initialize the custom implementations
var customProcGen = new ProceduralGenerator<int, Cell<int>, TestChunkData>(Seed, chunkGenerationMethod);
var customGrid = new Grid<int, Cell<int>, TestChunkData>(ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight, customProcGen);

// Retrieve the chunk data, for the whole chunk where position (5, 5) resides in
var chunkData = customGrid.GetChunkData(5, 5);
Console.WriteLine("Trees in chunk: " + chunkData.Trees != null ? chunkData.Trees.Count : 0);
```
You can store the chunkdata within the internal cache buffer, which GetChunkData will then return instead
```csharp
customGrid.StoreChunkData(chunkData);
customGrid.RemoveChunkData(chunkData, reloadChunk); // chunkdata only refreshes after chunk is reloaded
```

# Rendering to a render engine
FlowVitae provides an event that is raised when a cell on the viewport is updated
```csharp
grid.OnCellUpdate += Grid_OnCellUpdate;

private void Grid_OnCellUpdate(object? sender, CellUpdateArgs<int, Cell<int>> args)
{
  // Pseudo code
  var screenGraphic = ConvertCellTypeToGraphic(args.Cell.CellType);
  SomeRenderEngine.SetScreenGraphic(args.ScreenX, args.ScreenY, screenGraphic);
}
```
This event is by default only raised when the TCellType value on the viewport is changed during a SetCell/SetCells
If you want this event to always be raised when a TCell is set, (even if CellType doesn't change, but some properties do)
Then we also provided this functionality. You can adjust it like so:
```csharp
grid.RaiseOnlyOnCellTypeChange = false;
```

# Custom cell conversion
There are some ways to convert the underlying TCellType to TCell.
- You can implement your own Grid class based on the GridBase<TCellType, TCell> and override the Convert method.
- You can call the method SetCustomConverter(converter) on the Grid which is then used in Convert instead of default new() constructor
	
Here is an example of the method:
```csharp
_grid.SetCustomConverter(WorldGenerator.CellConverter);
	
public Cell<int> ConvertCell(int x, int y, int cellType)
{
	switch (cellType)
	{
		case 0:
			return new Cell<int>(x, y, cellType);
		case 1:
			return new Cell<int>(x, y, walkable: false, cellType);
		default:
			return new Cell<int>(x, y, walkable: false, cellType);
	}
}
```
	
# Creating your own Cell implementation
It can be easily done by inheriting from CellBase or ICell<TCellType>
	
When you want your cell to be able to base of some render engine cell such as ColoredGlyph from Sadconsole,
you can easily do it by using ColoredGlyph, ICell<TCellType> as your inheritance.
	
Here is an example of just a regular CellBase inheritance:
```csharp
internal class VisualCell<TCellType> : CellBase<TCellType>
where TCellType : struct
{
	public bool Walkable { get; set; } = true;
	public bool BlocksFieldOfView { get; set; } // Some custom properties
	public bool HasLightSource { get; set; } // Some custom properties

	public VisualCell() { }

	public VisualCell(int x, int y, TCellType cellType)
	{
		X = x;
		Y = y;
		CellType = cellType;
	}
}
```

# Interaction with grids

**Getting and setting cells**
```csharp
var cell = grid.GetCell(x, y); // returns TCell
var cellType = grid.GetCelLType(x,y); // returns TCellType
var neighbors = grid.GetNeighbors(x, y, AdjacencyRule);
grid.SetCell(x, y, cellType, storeState);
grid.SetCell(cell, storeState);
var cells = grid.GetCells(new [] {(0,0), (1,1)}); // returns collection of TCell
grid.SetCells(cells, storeState);
```

**Center viewport on a coordinate for procedural grids**
	
This is especially useful when you want your player to always be centered in the middle of the screen.
But during movement, the viewport adjusts to show the right cells based on the position of the player
For this you can use the Center(x, y) method Grid provides. This method is also what controls the chunk loading.
```csharp
// Pseudo code (make sure player doesn't actually move, or you'll end up with desync)
if (player.MovedTowards(x, y))
    grid.Center(x, y);
```

**Retrieve all cells within the viewport**
```csharp
// Returns all world positions that are within the current viewport
grid.GetViewPortWorldCoordinates();
grid.GetViewPortWorldCoordinates(cellType => cellType == 1 || cellType == 2); // with custom criteria
```

**Checking bounds for static grids**
```csharp
// Returns true or false if the position is within the viewport
// Works only for screen coordinates if you're using a chunked grid
var isInBounds = grid.InBounds(x, y);
```

**See if a cell is currently displayed on the viewport**
```csharp
var isInViewPort = grid.IsWorldCoordinateOnViewPort(x,y);
```

**Reset grid state**

```csharp
grid.ClearCache(); // Removes all stored cell data
```

**Be notified of main chunk loading/unloading**

Following events will be raised when one of the chunks around the center chunk (center chunk included) gets loaded or unloaded.
```csharp
OnChunkLoad
OnChunkUnload
```

Some chunk related methods: (x, y) is automatically converted to a chunk coordinate, so it can take any world position.
```csharp
Grid.GetChunkSeed(x, y);
Grid.IsChunkLoaded(x, y);
Grid.GetChunkCoordinate(x, y);
Grid.GetChunkCellCoordinates(x, y);
Grid.GetLoadedChunkCoordinates();
```

# Integration with SadConsole

Checkout the Visualizer project, it is an example project that integrates the FlowVitae grid with SadConsole V9 render engine.
