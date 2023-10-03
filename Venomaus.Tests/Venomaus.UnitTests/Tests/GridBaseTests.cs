using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Chunking;
using Venomaus.FlowVitae.Grids;
using Venomaus.FlowVitae.Helpers;
using Venomaus.UnitTests.Tools;

namespace Venomaus.UnitTests.Tests
{
    [TestFixture(25, 25, 25, 25)]
    [TestFixture(17, 28, 17, 28)]
    [TestOf(typeof(GridBase<int, Cell<int>>))]
    [Parallelizable(ParallelScope.Fixtures)]
    internal class GridBaseTests : BaseTests<int, Cell<int>>
    {
        public GridBaseTests(int viewPortWidth, int viewPortHeight, int chunkWidth, int chunkHeight)
        {
            ViewPortWidth = viewPortWidth;
            ViewPortHeight = viewPortHeight;
            ChunkWidth = chunkWidth;
            ChunkHeight = chunkHeight;
        }

        [Test]
        public void ChunkLoader_Is_Null()
        {
            var grid = CreateNewGrid();
            var chunkLoader = () => grid._chunkLoader ?? throw new Exception("No chunkloader available");
            Assert.Multiple(() =>
            {
                Assert.That(grid._chunkLoader, Is.Null);
                Assert.That(() => chunkLoader.Invoke(), Throws.Exception);
            });
        }

        [Test]
        public void Grid_Constructors_Covered()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => new Grid<int, Cell<int>>(100, 100), Throws.Nothing);
                Assert.That(() => new Grid<int, Cell<int>>(100, 100, 50, 50, null), Throws.Nothing);
                Assert.That(() => new Grid<int, Cell<int>>(100, 100, null), Throws.Nothing);
                Assert.That(() => new Grid<int, Cell<int>>(100, 100, ProcGen), Throws.Nothing);
                Assert.That(() => new Grid<int, Cell<int>>(100, 100, 50, 50, ProcGen), Throws.Nothing);
                Assert.That(() => new Grid<int, Cell<int>, IChunkData>(100, 100), Throws.Nothing);
                Assert.That(() => new Grid<int, Cell<int>, IChunkData>(100, 100, ProcGen), Throws.Nothing);
                Assert.That(() => new Grid<int, Cell<int>, IChunkData>(100, 100, 50, 50, ProcGen), Throws.Nothing);
                Assert.That(() => new Grid<int, Cell<int>, IChunkData>(100, 100, 50, 50, null), Throws.Nothing);
                Assert.That(() => new Grid<int, Cell<int>, IChunkData>(100, 100, null), Throws.Nothing);
            });
        }

        [Test]
        public void GetCell_Get_Correct()
        {
            var grid = CreateNewGrid();
            var cell = grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell, Is.EqualTo(new Cell<int>(5, 5)));

            cell = grid.GetCell(-5, -5);
            Assert.That(cell, Is.Null);

            cell = grid.GetCell(grid.Width + 5, grid.Height + 5);
            Assert.That(cell, Is.Null);
        }

        [Test]
        public void GetCells_Get_Correct()
        {
            var grid = CreateNewGrid();
            var cellPositions = new[] { (5, 5), (3, 2), (4, 4) };
            var cells = grid.GetCells(cellPositions).ToArray();
            Assert.That(cells, Has.Length.EqualTo(cellPositions.Length));

            for (int i = 0; i < cellPositions.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    var cell = cells[i];
                    Assert.That(cell, Is.Not.Null);
                    if (cell != null)
                    {
                        Assert.That(cell.X, Is.EqualTo(cellPositions[i].Item1));
                        Assert.That(cell.Y, Is.EqualTo(cellPositions[i].Item2));
                    }
                });
            }
        }

        [Test]
        public void SetCells_GetCells_SetGet_Correct()
        {
            var grid = CreateNewGrid();
            var cells = new[] 
            { 
                new Cell<int>(5, 5, false, 1), 
                new Cell<int>(3, 2, false, 2), 
                new Cell<int>(4, 4, true, 3) 
            };
            Assert.That(() => grid.SetCells(cells, true), Throws.Nothing);

            var newCells = grid.GetCells(cells.Select(a => (a.X, a.Y))).ToArray();
            Assert.That(newCells, Has.Length.EqualTo(cells.Length));

            for (int i = 0; i < cells.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    var newCell = newCells[i];
                    Assert.That(newCell, Is.Not.Null);
                    if (newCell != null)
                    {
                        Assert.That(newCell.X, Is.EqualTo(cells[i].X));
                        Assert.That(newCell.Y, Is.EqualTo(cells[i].Y));
                        Assert.That(newCell.CellType, Is.EqualTo(cells[i].CellType), "Cell type is invalid");
                        Assert.That(newCell.Walkable, Is.EqualTo(cells[i].Walkable), "Cell number is invalid");
                    }
                });
            }

            Assert.That(() => grid.SetCells(cells, false), Throws.Nothing);
            newCells = grid.GetCells(cells.Select(a => (a.X, a.Y))).ToArray();
            Assert.That(newCells, Has.Length.EqualTo(cells.Length));

            for (int i = 0; i < cells.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    var newCell = newCells[i];
                    Assert.That(newCell, Is.Not.Null);
                    if (newCell != null)
                    {
                        Assert.That(newCell.X, Is.EqualTo(cells[i].X));
                        Assert.That(newCell.Y, Is.EqualTo(cells[i].Y));
                        Assert.That(newCell.CellType, Is.EqualTo(cells[i].CellType), "Cell type is invalid");
                        Assert.That(newCell.Walkable, Is.EqualTo(true), "Cell number is invalid");
                    }
                });
            }
        }

        [Test]
        public void SetCell_Set_Correct()
        {
            var grid = CreateNewGrid();
            var cell = grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(0));

            grid.SetCell(5, 5, 1);

            var changedCell = grid.GetCell(5, 5);
            Assert.That(changedCell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(changedCell.CellType, Is.EqualTo(1));
                Assert.That(() => grid.SetCell(grid.Width + 5, grid.Height + 5, 1), Throws.Nothing);
                Assert.That(() => grid.SetCell(-5, -5, 1), Throws.Nothing);
            });
        }

        [Test]
        public void GetCellType_Get_Correct()
        {
            var grid = CreateNewGrid();
            var cellType = grid.GetCellType(grid.Width / 2, grid.Height / 2);
            Assert.That(cellType, Is.Not.EqualTo(-1));

            grid.SetCell(grid.Width / 2, grid.Height / 2, -1);

            cellType = grid.GetCellType(grid.Width / 2, grid.Height / 2);
            Assert.That(cellType, Is.EqualTo(-1));

            grid.SetCell(grid.Width / 2, grid.Height / 2, -1, true);

            cellType = grid.GetCellType(grid.Width / 2, grid.Height / 2);
            Assert.That(cellType, Is.EqualTo(-1));

            cellType = grid.GetCellType(grid.Width * 5, grid.Height * 5);
            Assert.That(cellType, Is.EqualTo(default(int)));
        }

        [Test]
        public void StoreState_SetAndGet_Correct()
        {
            var grid = CreateNewGrid();
            var cell = grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(0));

            var customCell = new Cell<int>(5, 5, false, 20);
            grid.SetCell(customCell, true);

            Assert.That(() => grid.SetCell(null), Throws.Nothing);

            var changedCell = grid.GetCell(5, 5);
            Assert.That(changedCell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(changedCell.CellType, Is.EqualTo(customCell.CellType));
                Assert.That(changedCell.Walkable, Is.EqualTo(customCell.Walkable));
            });

            grid.SetCell(5, 5, 1, true);

            changedCell = grid.GetCell(5, 5);
            Assert.That(changedCell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(changedCell.CellType, Is.EqualTo(1));
                Assert.That(changedCell.Walkable, Is.EqualTo(true));
            });
        }

        [Test]
        public void InBounds_Get_Correct()
        {
            var grid = CreateNewGrid();
            var inBoundsTrue = grid.InBounds(5, 5);
            Assert.That(inBoundsTrue, Is.EqualTo(true));

            var inBoundsFalse = grid.InBounds(-5, 30);
            Assert.That(inBoundsFalse, Is.EqualTo(false));

            Assert.That(grid.InBounds(null), Is.False);
        }

        [Test]
        public void ScreenToWorldPoint_Get_Correct()
        {
            var grid = CreateNewGrid();
            var screenPosX = 5;
            var screenPosY = 5;
            var (x, y) = grid.ScreenToWorldCoordinate(screenPosX, screenPosY);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(screenPosX));
                Assert.That(y, Is.EqualTo(screenPosY));
                Assert.That(() => grid.ScreenToWorldCoordinate(grid.Width + 5, grid.Height + 5), Throws.Exception);
                Assert.That(() => grid.ScreenToWorldCoordinate(-5, -5), Throws.Exception);
            });
        }

        [Test]
        public void WorldToScreenPoint_Get_Correct()
        {
            var grid = CreateNewGrid();
            var worldPosX = 5;
            var worldPosY = 5;
            var (x, y) = grid.WorldToScreenCoordinate(worldPosX, worldPosY);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(worldPosX));
                Assert.That(y, Is.EqualTo(worldPosY));
                Assert.That(() => grid.WorldToScreenCoordinate(grid.Width + 5, grid.Height + 5), Throws.Exception);
                Assert.That(() => grid.WorldToScreenCoordinate(-5, -5), Throws.Exception);
            });
        }

        [Test]
        public void GetViewPortCells_Get_Correct()
        {
            var grid = CreateNewGrid();
            var viewPort = grid.GetViewPortWorldCoordinates().ToArray();
            Assert.That(viewPort, Has.Length.EqualTo(grid.Width * grid.Height));

            // Check if the order is also correct
            var comparer = new TupleComparer<int>();
            for (int x=0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var index = y * grid.Width + x;
                    Assert.That(comparer.Equals(viewPort[index], (x, y)));  
                }
            }
        }

        [Test]
        public void IsWorldCoordinateOnViewPort_Get_Correct()
        {
            var grid = CreateNewGrid();
            Assert.Multiple(() =>
            {
                Assert.That(grid.IsWorldCoordinateOnViewPort(5, 5), Is.EqualTo(true));
                Assert.That(grid.IsWorldCoordinateOnViewPort(-5, -5), Is.EqualTo(false));
                Assert.That(grid.IsWorldCoordinateOnViewPort(grid.Width + 5, grid.Height + 5), Is.EqualTo(false));
            });
        }

        [Test]
        public void OnCellUpdate_SetCell_Raised_Correct()
        {
            var grid = CreateNewGrid();
            object? sender = null;
            CellUpdateArgs<int, Cell<int>>? args = null;
            grid.OnCellUpdate += (cellSender, cellArgs) =>
            {
                sender = cellSender;
                args = cellArgs;
            };

            // Set cell within the view port
            grid.SetCell(new Cell<int>(grid.Width / 2, grid.Height / 2, false, -1));

            // Verify if args are received properly
            Assert.That(args, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(sender, Is.Null);
                Assert.That(args.ScreenX, Is.EqualTo(grid.Width / 2));
                Assert.That(args.ScreenY, Is.EqualTo(grid.Height / 2));
                Assert.That(args.Cell, Is.Not.Null);
                if (args.Cell != null)
                {
                    Assert.That(args.Cell.X, Is.EqualTo(grid.Width / 2));
                    Assert.That(args.Cell.Y, Is.EqualTo(grid.Height / 2));
                    Assert.That(args.Cell.CellType, Is.EqualTo(-1));
                    Assert.That(args.Cell.Walkable, Is.EqualTo(false));
                }
            });

            args = null;

            // Set cell within the view port, but no cell type change
            grid.SetCell(new Cell<int>(grid.Width / 2, grid.Height / 2, false, -1));
            // Verify no args are received
            Assert.That(args, Is.Null);

            // Set cell within the view port, but no cell type change, with adjusted raise flag
            grid.RaiseOnlyOnCellTypeChange = false;
            grid.SetCell(new Cell<int>(grid.Width / 2, grid.Height / 2, false, -1));

            // Verify if args are received properly
            Assert.That(args, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(sender, Is.Null);
                Assert.That(args.ScreenX, Is.EqualTo(grid.Width / 2));
                Assert.That(args.ScreenY, Is.EqualTo(grid.Height / 2));
                Assert.That(args.Cell, Is.Not.Null);
                if (args.Cell != null)
                {
                    Assert.That(args.Cell.X, Is.EqualTo(grid.Width / 2));
                    Assert.That(args.Cell.Y, Is.EqualTo(grid.Height / 2));
                    Assert.That(args.Cell.CellType, Is.EqualTo(-1));
                    Assert.That(args.Cell.Walkable, Is.EqualTo(false));
                }
            });

            args = null;

            // Set cell outside of the view port
            grid.SetCell(new Cell<int>(grid.Width + 5, grid.Height + 5, false, -2));
            // Verify no args are received
            Assert.That(args, Is.Null);
        }

        [Test]
        public void OnCellUpdate_SetCells_Raised_Correct()
        {
            var grid = CreateNewGrid();
            object? sender = null;
            CellUpdateArgs<int, Cell<int>>? args = null;
            grid.OnCellUpdate += (cellSender, cellArgs) =>
            {
                sender = cellSender;
                args = cellArgs;
            };

            // Set cell within the view port
            grid.SetCells(new[] { new Cell<int>(grid.Width / 2, grid.Height / 2, false, -1) });

            // Verify if args are received properly
            Assert.That(args, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(sender, Is.Null);
                Assert.That(args.ScreenX, Is.EqualTo(grid.Width / 2));
                Assert.That(args.ScreenY, Is.EqualTo(grid.Height / 2));
                Assert.That(args.Cell, Is.Not.Null);
                if (args.Cell != null)
                {
                    Assert.That(args.Cell.X, Is.EqualTo(grid.Width / 2));
                    Assert.That(args.Cell.Y, Is.EqualTo(grid.Height / 2));
                    Assert.That(args.Cell.CellType, Is.EqualTo(-1));
                    Assert.That(args.Cell.Walkable, Is.EqualTo(false));
                }
            });

            args = null;

            // Set cell within the view port, but no cell type change
            grid.SetCells(new[] { new Cell<int>(grid.Width / 2, grid.Height / 2, false, -1) });
            // Verify no args are received
            Assert.That(args, Is.Null);

            // Set cell within the view port, but no cell type change, with adjusted raise flag
            grid.RaiseOnlyOnCellTypeChange = false;
            grid.SetCells(new[] { new Cell<int>(grid.Width / 2, grid.Height / 2, false, -1) });

            // Verify if args are received properly
            Assert.That(args, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(sender, Is.Null);
                Assert.That(args.ScreenX, Is.EqualTo(grid.Width / 2));
                Assert.That(args.ScreenY, Is.EqualTo(grid.Height / 2));
                Assert.That(args.Cell, Is.Not.Null);
                if (args.Cell != null)
                {
                    Assert.That(args.Cell.X, Is.EqualTo(grid.Width / 2));
                    Assert.That(args.Cell.Y, Is.EqualTo(grid.Height / 2));
                    Assert.That(args.Cell.CellType, Is.EqualTo(-1));
                    Assert.That(args.Cell.Walkable, Is.EqualTo(false));
                }
            });

            args = null;

            // Set cell outside of the view port
            grid.SetCells(new[] { new Cell<int>(grid.Width + 5, grid.Height + 5, false, -2) });
            // Verify no args are received
            Assert.That(args, Is.Null);
        }

        [Test]
        public void SetCustomConverter_Converts_Correct()
        {
            var grid = CreateNewGrid();
            Assert.Multiple(() =>
            {
                Assert.That(() => grid.SetCustomConverter((int x, int y, int cellType) =>
                {
                    return new Cell<int>(x, y, cellType != -1, cellType);
                }), Throws.Nothing);

                Assert.That(() => grid.SetCell(5, 5, -1), Throws.Nothing);
            });

            var cell = grid.GetCell(5, 5);

            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.Walkable, Is.EqualTo(false));
        }

        [Test]
        public void CompareCellToCoordinate_Correct()
        {
            Cell<int> cell = new()
            {
                X = 5,
                Y = 5
            };
            Cell<int> cell2 = new(5, 5, -5);
            var coordinate = (5, 5);
            Assert.That(cell.Equals(coordinate), Is.True);
            coordinate = (4, 2);
            Assert.That(cell.Equals(coordinate), Is.False);
            Assert.That(cell.Equals(cell2), Is.True);
            Assert.That(cell.Equals(null), Is.False);
        }

        [Test]
        public void SetZeroViewportSize_Throws_Exception()
        {
            Grid<int, Cell<int>>? newGrid = null;
            Assert.Multiple(() =>
            {
                Assert.That(() => newGrid = new Grid<int, Cell<int>>(0, 0), Throws.Exception);
                Assert.That(newGrid, Is.Null);
            });
        }

        [Test]
        public void Center_DoesNot_Throw()
        {
            var grid = CreateNewGrid();
            Assert.That(() => grid.Center(0, 0), Throws.Nothing);
        }

        [Test]
        public void ClearGridCache_Throws_NoException()
        {
            var grid = CreateNewGrid();
            // Populate the grid cache
            var cells = new List<Cell<int>?>();
            for (int x = grid.Width / 2; x < (grid.Width / 2) + 10; x++)
            {
                for (int y = grid.Height / 2; y < (grid.Height / 2) + 10; y++)
                {
                    cells.Add(new Cell<int>(x, y, false, -10));
                }
            }

            List<Cell<int>?> prevState = grid.GetCells(cells.Where(a => a != null).Cast<Cell<int>>().Select(a => (a.X, a.Y))).ToList();
            grid.SetCells(cells, true);

            Assert.That(() => grid.ClearCache(), Throws.Nothing);

            cells = grid.GetCells(cells.Where(a => a != null).Cast<Cell<int>>().Select(a => (a.X, a.Y))).ToList();

            Assert.That(cells.SequenceEqual(prevState, new CellWalkableComparer<int>()));
        }

        [Test]
        public void ChunkDataRelatedMethods_DoNothing()
        {
            var grid = CreateNewGrid();
            Assert.That(() => grid.StoreChunkData(new TestChunkData()), Throws.Nothing);
            Assert.That(() => grid.RemoveChunkData(new TestChunkData()), Throws.Nothing);
            Assert.That(grid.GetChunkData(0, 0), Is.Null);
        }

        [Test]
        public void UseThreading_DoesNotWork_StaticGrid()
        {
            var grid = CreateNewGrid();
            grid.UseThreading = true;
            Assert.That(grid.UseThreading, Is.False);
        }

        [Test]
        public void GetNeighbors_Retrieves_CorrectCells()
        {
            var grid = CreateNewGrid();
            var neighbors4Way = grid.GetNeighbors(5, 5, AdjacencyRule.FourWay);
            Assert.That(neighbors4Way.Count(), Is.EqualTo(4));

            var comparer = new TupleComparer<int>();
            // Verify neighbors retrieved are correct
            var correctNeighbors = new[]
            {
                (4,5), (6,5),
                (5, 4), (5, 6)
            };
            Assert.That(neighbors4Way.Where(a => a != null).Cast<Cell<int>>().Select(a => (a.X, a.Y)).SequenceEqual(correctNeighbors, comparer));

            var neighbors8Way = grid.GetNeighbors(5, 5, AdjacencyRule.EightWay);
            Assert.That(neighbors8Way.Count(), Is.EqualTo(8));

            // Verify neighbors retrieved are correct
            correctNeighbors = new[]
            {
                (4,4), (4,5),
                (4, 6), (5, 4),
                (5,6), (6,4),
                (6, 5), (6, 6)
            };
            Assert.That(neighbors8Way.Where(a => a != null).Cast<Cell<int>>().Select(a => (a.X, a.Y)).SequenceEqual(correctNeighbors, comparer));
        }

        [Test]
        public void GetNeighbor_OutOfBounds_Retrieves_CorrectCells()
        {
            var grid = CreateNewGrid();
            var neighbors4Way = grid.GetNeighbors(0, 0, AdjacencyRule.FourWay);
            Assert.That(neighbors4Way.Count(), Is.EqualTo(2));

            var comparer = new TupleComparer<int>();
            // Verify neighbors retrieved are correct
            var correctNeighbors = new[]
            {
                (1,0), (0,1),
            };
            Assert.That(neighbors4Way.Where(a => a != null).Cast<Cell<int>>().Select(a => (a.X, a.Y)).SequenceEqual(correctNeighbors, comparer));

            var neighbors8Way = grid.GetNeighbors(0, 0, AdjacencyRule.EightWay);
            Assert.That(neighbors8Way.Count(), Is.EqualTo(3));

            // Verify neighbors retrieved are correct
            correctNeighbors = new[]
            {
                (0,1), (1,0), (1,1)
            };
            Assert.That(neighbors8Way.Where(a => a != null).Cast<Cell<int>>().Select(a => (a.X, a.Y)).SequenceEqual(correctNeighbors, comparer));
        }

        [Test]
        public void GetChunkSeed_Returns_CorrectValue()
        {
            var grid = CreateNewGrid();
            var chunkSeed = grid.GetChunkSeed(0, 0);
            Assert.That(chunkSeed, Is.EqualTo(0));
        }

        [Test]
        public void IsChunkLoaded_Returns_CorrectValue()
        {
            var grid = CreateNewGrid();
            var chunkLoaded = grid.IsChunkLoaded(ViewPortWidth / 2, ViewPortHeight / 2);
            Assert.That(chunkLoaded, Is.False);
        }

        [Test]
        public void GetChunkCoordinate_ReturnsResult_Correct()
        {
            var grid = CreateNewGrid();
            var comparer = new TupleComparer<int>();
            var coord = grid.GetChunkCoordinate(ChunkWidth / 2, ChunkHeight / 2);
            Assert.That(comparer.Equals(coord, (ChunkWidth / 2, ChunkHeight / 2)));
        }

        [Test]
        public void GetLoadedChunkCoordinates_ReturnsResult_Correct()
        {
            var grid = CreateNewGrid();
            var loadedChunks = grid.GetLoadedChunkCoordinates();
            Assert.That(loadedChunks.Count(), Is.EqualTo(0));
        }

        [Test]
        public void GetChunkCellCoordinates_ReturnsResult_Correct()
        {
            var grid = CreateNewGrid();
            var loadedChunks = grid.GetChunkCellCoordinates(0, 0).ToArray();
            Assert.That(loadedChunks, Has.Length.EqualTo(1));
            var comparer = new TupleComparer<int>();
            Assert.That(comparer.Equals(loadedChunks[0], (0, 0)));
        }

        [Test]
        public void HasStoredCell_ReturnsResult_Correct()
        {
            var grid = CreateNewGrid();
            // Out of bounds test
            Assert.That(grid.HasStoredCell(-5, -2), Is.False);

            Assert.That(grid.HasStoredCell(0, 0), Is.False);
            grid.SetCell(new Cell<int>(0, 0, -50), true);
            Assert.That(grid.HasStoredCell(0, 0), Is.True);
            grid.SetCell(new Cell<int>(0, 0, -50), false);
            Assert.That(grid.HasStoredCell(0, 0), Is.False);
        }

        [Test]
        public void RemoveStoredCell_ReturnsResult_Correct()
        {
            var grid = CreateNewGrid();
            Assert.That(grid.HasStoredCell(0, 0), Is.False);
            grid.SetCell(new Cell<int>(0, 0, -50), true);
            Assert.That(grid.HasStoredCell(0, 0), Is.True);
            grid.RemoveStoredCell(0, 0);
            Assert.That(grid.HasStoredCell(0, 0), Is.False);
        }

        [Test]
        public void GetCells_CanReturn_NullValues()
        {
            var grid = CreateNewGrid();
            grid.SetCustomConverter((x, y, cellType) =>
            {
                return cellType != -1 ? new Cell<int>(x, y, cellType) : null;
            });

            (int x, int y) pos = (grid.Width / 2, grid.Height / 2);
            grid.SetCell(pos.x, pos.y, -1);

            // Check for null
            var cell = grid.GetCell(pos.x, pos.y);
            Assert.That(cell, Is.Null);

            var cells = grid.GetCells(new[] { pos }).ToArray();
            Assert.That(cells, Is.Not.Null);
            Assert.That(cells, Has.Length.EqualTo(1));
            Assert.That(cells[0], Is.Null);

            // Check for not null
            cell = grid.GetCell(pos.x + 1, pos.y + 1);
            Assert.That(cell, Is.Not.Null);

            cells = grid.GetCells(new[] { (pos.x + 1, pos.y + 1) }).ToArray();
            Assert.That(cells, Is.Not.Null);
            Assert.That(cells, Has.Length.EqualTo(1));
            Assert.That(cells[0], Is.Not.Null);
        }
    }
}