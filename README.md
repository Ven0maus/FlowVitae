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
public void GenerateChunkMethod(Random random, int[] chunk, int width, int height)
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

Chunks are generated automatically and they will use this method as reference to build the chunk.

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
grid.SetCell(x, y);
var cells = grid.GetCells(new [] {(0,0), (1,1)}); // returns collection of TCell
grid.SetCells(cells);
```

**Center viewport on a coordinate for procedural grids**
	
This is especially useful when you want your player to always be centered in the middle of the screen.
But during movement, the viewport adjusts to show the right cells based on the position of the player
For this you can use the Center(x, y) method Grid provides.
```csharp
// Pseudo code (make sure player doesn't actually move, or you'll end up with desync)
if (player.MovedTowards(x, y))
    grid.Center(x, y);
```

**Retrieve all cells within the viewport**
```csharp
// Returns a cloned array of the viewport, all cell positions are in screen coordinates instead of world coordinates
grid.GetViewPortCells();
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

# Integration with SadConsole

Checkout the Visualizer project, it is an example project that integrates the FlowVitae grid with SadConsole V9 render engine.
