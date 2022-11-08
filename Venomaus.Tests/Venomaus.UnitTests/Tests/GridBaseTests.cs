using Venomaus.FlowVitae.Chunking;
using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Grids;
using Venomaus.FlowVitae.Helpers;
using Venomaus.UnitTests.Tools;

namespace Venomaus.UnitTests.Tests
{
    [TestFixture(25, 25, 25, 25)]
    [TestFixture(17, 28, 17, 28)]
    [TestOf(typeof(GridBase<int, Cell<int>>))]
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
            Assert.Multiple(() =>
            {
                Assert.That(Grid._chunkLoader, Is.Null);
                Assert.That(() => ChunkLoader, Throws.Exception);
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
            var cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell, Is.EqualTo(new Cell<int>(5, 5)));

            cell = Grid.GetCell(-5, -5);
            Assert.That(cell, Is.Null);

            cell = Grid.GetCell(Grid.Width + 5, Grid.Height + 5);
            Assert.That(cell, Is.Null);
        }

        [Test]
        public void GetCells_Get_Correct()
        {
            var cellPositions = new[] { (5, 5), (3, 2), (4, 4) };
            var cells = Grid.GetCells(cellPositions).ToArray();
            Assert.That(cells, Has.Length.EqualTo(cellPositions.Length));

            for (int i = 0; i < cellPositions.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(cells[i].X, Is.EqualTo(cellPositions[i].Item1));
                    Assert.That(cells[i].Y, Is.EqualTo(cellPositions[i].Item2));
                });
            }
        }

        [Test]
        public void SetCells_GetCells_SetGet_Correct()
        {
            var cells = new[] 
            { 
                new Cell<int>(5, 5, false, 1), 
                new Cell<int>(3, 2, false, 2), 
                new Cell<int>(4, 4, true, 3) 
            };
            Assert.That(() => Grid.SetCells(cells, true), Throws.Nothing);

            var newCells = Grid.GetCells(cells.Select(a => (a.X, a.Y))).ToArray();
            Assert.That(newCells, Has.Length.EqualTo(cells.Length));

            for (int i = 0; i < cells.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(newCells[i].X, Is.EqualTo(cells[i].X));
                    Assert.That(newCells[i].Y, Is.EqualTo(cells[i].Y));
                    Assert.That(newCells[i].CellType, Is.EqualTo(cells[i].CellType), "Cell type is invalid");
                    Assert.That(newCells[i].Walkable, Is.EqualTo(cells[i].Walkable), "Cell number is invalid");
                });
            }

            Assert.That(() => Grid.SetCells(cells, false), Throws.Nothing);
            newCells = Grid.GetCells(cells.Select(a => (a.X, a.Y))).ToArray();
            Assert.That(newCells, Has.Length.EqualTo(cells.Length));

            for (int i = 0; i < cells.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(newCells[i].X, Is.EqualTo(cells[i].X));
                    Assert.That(newCells[i].Y, Is.EqualTo(cells[i].Y));
                    Assert.That(newCells[i].CellType, Is.EqualTo(cells[i].CellType), "Cell type is invalid");
                    Assert.That(newCells[i].Walkable, Is.EqualTo(true), "Cell number is invalid");
                });
            }
        }

        [Test]
        public void SetCell_Set_Correct()
        {
            var cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(0));

            Grid.SetCell(5, 5, 1);

            var changedCell = Grid.GetCell(5, 5);
            Assert.That(changedCell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(changedCell.CellType, Is.EqualTo(1));
                Assert.That(() => Grid.SetCell(Grid.Width + 5, Grid.Height + 5, 1), Throws.Nothing);
                Assert.That(() => Grid.SetCell(-5, -5, 1), Throws.Nothing);
            });
        }

        [Test]
        public void GetCellType_Get_Correct()
        {
            var cellType = Grid.GetCellType(Grid.Width / 2, Grid.Height / 2);
            Assert.That(cellType, Is.Not.EqualTo(-1));

            Grid.SetCell(Grid.Width / 2, Grid.Height / 2, -1);

            cellType = Grid.GetCellType(Grid.Width / 2, Grid.Height / 2);
            Assert.That(cellType, Is.EqualTo(-1));

            Grid.SetCell(Grid.Width / 2, Grid.Height / 2, -1, true);

            cellType = Grid.GetCellType(Grid.Width / 2, Grid.Height / 2);
            Assert.That(cellType, Is.EqualTo(-1));

            cellType = Grid.GetCellType(Grid.Width * 5, Grid.Height * 5);
            Assert.That(cellType, Is.EqualTo(default(int)));
        }

        [Test]
        public void StoreState_SetAndGet_Correct()
        {
            var cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(0));

            var customCell = new Cell<int>(5, 5, false, 20);
            Grid.SetCell(customCell, true);

            var changedCell = Grid.GetCell(5, 5);
            Assert.That(changedCell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(changedCell.CellType, Is.EqualTo(customCell.CellType));
                Assert.That(changedCell.Walkable, Is.EqualTo(customCell.Walkable));
            });

            Grid.SetCell(5, 5, 1, true);

            changedCell = Grid.GetCell(5, 5);
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
            var inBoundsTrue = Grid.InBounds(5, 5);
            Assert.That(inBoundsTrue, Is.EqualTo(true));

            var inBoundsFalse = Grid.InBounds(-5, 30);
            Assert.That(inBoundsFalse, Is.EqualTo(false));
        }

        [Test]
        public void ScreenToWorldPoint_Get_Correct()
        {
            var screenPosX = 5;
            var screenPosY = 5;
            var (x, y) = Grid.ScreenToWorldCoordinate(screenPosX, screenPosY);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(screenPosX));
                Assert.That(y, Is.EqualTo(screenPosY));
                Assert.That(() => Grid.ScreenToWorldCoordinate(Grid.Width + 5, Grid.Height + 5), Throws.Exception);
                Assert.That(() => Grid.ScreenToWorldCoordinate(-5, -5), Throws.Exception);
            });
        }

        [Test]
        public void WorldToScreenPoint_Get_Correct()
        {
            var worldPosX = 5;
            var worldPosY = 5;
            var (x, y) = Grid.WorldToScreenCoordinate(worldPosX, worldPosY);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(worldPosX));
                Assert.That(y, Is.EqualTo(worldPosY));
                Assert.That(() => Grid.WorldToScreenCoordinate(Grid.Width + 5, Grid.Height + 5), Throws.Exception);
                Assert.That(() => Grid.WorldToScreenCoordinate(-5, -5), Throws.Exception);
            });
        }

        [Test]
        public void GetViewPortCells_Get_Correct()
        {
            var viewPort = Grid.GetViewPortWorldCoordinates().ToArray();
            Assert.That(viewPort, Has.Length.EqualTo(Grid.Width * Grid.Height));
        }

        [Test]
        public void IsWorldCoordinateOnViewPort_Get_Correct()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Grid.IsWorldCoordinateOnViewPort(5, 5), Is.EqualTo(true));
                Assert.That(Grid.IsWorldCoordinateOnViewPort(-5, -5), Is.EqualTo(false));
                Assert.That(Grid.IsWorldCoordinateOnViewPort(Grid.Width + 5, Grid.Height + 5), Is.EqualTo(false));
            });
        }

        [Test]
        public void OnCellUpdate_SetCell_Raised_Correct()
        {
            object? sender = null;
            CellUpdateArgs<int, Cell<int>>? args = null;
            Grid.OnCellUpdate += (cellSender, cellArgs) =>
            {
                sender = cellSender;
                args = cellArgs;
            };

            // Set cell within the view port
            Grid.SetCell(new Cell<int>(Grid.Width / 2, Grid.Height / 2, false, -1));

            // Verify if args are received properly
            Assert.That(args, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(sender, Is.Null);
                Assert.That(args.ScreenX, Is.EqualTo(Grid.Width / 2));
                Assert.That(args.ScreenY, Is.EqualTo(Grid.Height / 2));
                Assert.That(args.Cell.X, Is.EqualTo(Grid.Width / 2));
                Assert.That(args.Cell.Y, Is.EqualTo(Grid.Height / 2));
                Assert.That(args.Cell.CellType, Is.EqualTo(-1));
                Assert.That(args.Cell.Walkable, Is.EqualTo(false));
            });

            args = null;

            // Set cell within the view port, but no cell type change
            Grid.SetCell(new Cell<int>(Grid.Width / 2, Grid.Height / 2, false, -1));
            // Verify no args are received
            Assert.That(args, Is.Null);

            // Set cell within the view port, but no cell type change, with adjusted raise flag
            Grid.RaiseOnlyOnCellTypeChange = false;
            Grid.SetCell(new Cell<int>(Grid.Width / 2, Grid.Height / 2, false, -1));

            // Verify if args are received properly
            Assert.That(args, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(sender, Is.Null);
                Assert.That(args.ScreenX, Is.EqualTo(Grid.Width / 2));
                Assert.That(args.ScreenY, Is.EqualTo(Grid.Height / 2));
                Assert.That(args.Cell.X, Is.EqualTo(Grid.Width / 2));
                Assert.That(args.Cell.Y, Is.EqualTo(Grid.Height / 2));
                Assert.That(args.Cell.CellType, Is.EqualTo(-1));
                Assert.That(args.Cell.Walkable, Is.EqualTo(false));
            });

            args = null;

            // Set cell outside of the view port
            Grid.SetCell(new Cell<int>(Grid.Width + 5, Grid.Height + 5, false, -2));
            // Verify no args are received
            Assert.That(args, Is.Null);
        }

        [Test]
        public void OnCellUpdate_SetCells_Raised_Correct()
        {
            object? sender = null;
            CellUpdateArgs<int, Cell<int>>? args = null;
            Grid.OnCellUpdate += (cellSender, cellArgs) =>
            {
                sender = cellSender;
                args = cellArgs;
            };

            // Set cell within the view port
            Grid.SetCells(new[] { new Cell<int>(Grid.Width / 2, Grid.Height / 2, false, -1) });

            // Verify if args are received properly
            Assert.That(args, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(sender, Is.Null);
                Assert.That(args.ScreenX, Is.EqualTo(Grid.Width / 2));
                Assert.That(args.ScreenY, Is.EqualTo(Grid.Height / 2));
                Assert.That(args.Cell.X, Is.EqualTo(Grid.Width / 2));
                Assert.That(args.Cell.Y, Is.EqualTo(Grid.Height / 2));
                Assert.That(args.Cell.CellType, Is.EqualTo(-1));
                Assert.That(args.Cell.Walkable, Is.EqualTo(false));
            });

            args = null;

            // Set cell within the view port, but no cell type change
            Grid.SetCells(new[] { new Cell<int>(Grid.Width / 2, Grid.Height / 2, false, -1) });
            // Verify no args are received
            Assert.That(args, Is.Null);

            // Set cell within the view port, but no cell type change, with adjusted raise flag
            Grid.RaiseOnlyOnCellTypeChange = false;
            Grid.SetCells(new[] { new Cell<int>(Grid.Width / 2, Grid.Height / 2, false, -1) });

            // Verify if args are received properly
            Assert.That(args, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(sender, Is.Null);
                Assert.That(args.ScreenX, Is.EqualTo(Grid.Width / 2));
                Assert.That(args.ScreenY, Is.EqualTo(Grid.Height / 2));
                Assert.That(args.Cell.X, Is.EqualTo(Grid.Width / 2));
                Assert.That(args.Cell.Y, Is.EqualTo(Grid.Height / 2));
                Assert.That(args.Cell.CellType, Is.EqualTo(-1));
                Assert.That(args.Cell.Walkable, Is.EqualTo(false));
            });

            args = null;

            // Set cell outside of the view port
            Grid.SetCells(new[] { new Cell<int>(Grid.Width + 5, Grid.Height + 5, false, -2) });
            // Verify no args are received
            Assert.That(args, Is.Null);
        }

        [Test]
        public void SetCustomConverter_Converts_Correct()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => Grid.SetCustomConverter((int x, int y, int cellType) =>
                {
                    return new Cell<int>(x, y, cellType != -1, cellType);
                }), Throws.Nothing);

                Assert.That(() => Grid.SetCell(5, 5, -1), Throws.Nothing);
            });

            var cell = Grid.GetCell(5, 5);

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
            Assert.That(() => Grid.Center(0, 0), Throws.Nothing);
        }

        [Test]
        public void ClearGridCache_Throws_NoException()
        {
            // Populate the grid cache
            var cells = new List<Cell<int>>();
            for (int x = Grid.Width / 2; x < (Grid.Width / 2) + 10; x++)
            {
                for (int y = Grid.Height / 2; y < (Grid.Height / 2) + 10; y++)
                {
                    cells.Add(new Cell<int>(x, y, false, -10));
                }
            }

            List<Cell<int>> prevState = Grid.GetCells(cells.Select(a => (a.X, a.Y))).ToList();
            Grid.SetCells(cells, true);

            Assert.That(() => Grid.ClearCache(), Throws.Nothing);

            cells = Grid.GetCells(cells.Select(a => (a.X, a.Y))).ToList();

            Assert.That(cells.SequenceEqual(prevState, new CellWalkableComparer<int>()));
        }

        [Test]
        public void ChunkDataRelatedMethods_DoNothing()
        {
            Assert.That(() => Grid.StoreChunkData(new TestChunkData()), Throws.Nothing);
            Assert.That(() => Grid.RemoveChunkData(new TestChunkData()), Throws.Nothing);
            Assert.That(Grid.GetChunkData(0, 0), Is.Null);
        }

        [Test]
        public void UseThreading_DoesNotWork_StaticGrid()
        {
            Grid.UseThreading = true;
            Assert.That(Grid.UseThreading, Is.False);
        }

        [Test]
        public void GetNeighbors_Retrieves_CorrectCells()
        {
            var neighbors4Way = Grid.GetNeighbors(5, 5, AdjacencyRule.FourWay);
            Assert.That(neighbors4Way.Count(), Is.EqualTo(4));

            var comparer = new TupleComparer<int>();
            // Verify neighbors retrieved are correct
            var correctNeighbors = new[]
            {
                (4,5), (6,5),
                (5, 4), (5, 6)
            };
            Assert.That(neighbors4Way.Select(a => (a.X, a.Y)).SequenceEqual(correctNeighbors, comparer));

            var neighbors8Way = Grid.GetNeighbors(5, 5, AdjacencyRule.EightWay);
            Assert.That(neighbors8Way.Count(), Is.EqualTo(8));

            // Verify neighbors retrieved are correct
            correctNeighbors = new[]
            {
                (4,4), (4,5),
                (4, 6), (5, 4),
                (5,6), (6,4),
                (6, 5), (6, 6)
            };
            Assert.That(neighbors8Way.Select(a => (a.X, a.Y)).SequenceEqual(correctNeighbors, comparer));
        }

        [Test]
        public void GetNeighbor_OutOfBounds_Retrieves_CorrectCells()
        {
            var neighbors4Way = Grid.GetNeighbors(0, 0, AdjacencyRule.FourWay);
            Assert.That(neighbors4Way.Count(), Is.EqualTo(2));

            var comparer = new TupleComparer<int>();
            // Verify neighbors retrieved are correct
            var correctNeighbors = new[]
            {
                (1,0), (0,1),
            };
            Assert.That(neighbors4Way.Select(a => (a.X, a.Y)).SequenceEqual(correctNeighbors, comparer));

            var neighbors8Way = Grid.GetNeighbors(0, 0, AdjacencyRule.EightWay);
            Assert.That(neighbors8Way.Count(), Is.EqualTo(3));

            // Verify neighbors retrieved are correct
            correctNeighbors = new[]
            {
                (0,1), (1,0), (1,1)
            };
            Assert.That(neighbors8Way.Select(a => (a.X, a.Y)).SequenceEqual(correctNeighbors, comparer));
        }

        [Test]
        public void GetChunkSeed_Returns_CorrectValue()
        {
            var chunkSeed = Grid.GetChunkSeed(0, 0);
            Assert.That(chunkSeed, Is.EqualTo(0));
        }

        [Test]
        public void IsChunkLoaded_Returns_CorrectValue()
        {
            var chunkLoaded = Grid.IsChunkLoaded(ViewPortWidth / 2, ViewPortHeight / 2);
            Assert.That(chunkLoaded, Is.False);
        }

        [Test]
        public void GetChunkCoordinate_ReturnsResult_Correct()
        {
            var comparer = new TupleComparer<int>();
            var coord = Grid.GetChunkCoordinate(ChunkWidth / 2, ChunkHeight / 2);
            Assert.That(comparer.Equals(coord, (ChunkWidth / 2, ChunkHeight / 2)));
        }

        [Test]
        public void GetLoadedChunkCoordinates_ReturnsResult_Correct()
        {
            var loadedChunks = Grid.GetLoadedChunkCoordinates();
            Assert.That(loadedChunks.Count(), Is.EqualTo(0));
        }

        [Test]
        public void GetChunkCellCoordinates_ReturnsResult_Correct()
        {
            var loadedChunks = Grid.GetChunkCellCoordinates(0, 0).ToArray();
            Assert.That(loadedChunks, Has.Length.EqualTo(1));
            var comparer = new TupleComparer<int>();
            Assert.That(comparer.Equals(loadedChunks[0], (0, 0)));
        }

        [Test]
        public void HasStoredCell_ReturnsResult_Correct()
        {
            // Out of bounds test
            Assert.That(Grid.HasStoredCell(-5, -2), Is.False);

            Assert.That(Grid.HasStoredCell(0, 0), Is.False);
            Grid.SetCell(new Cell<int>(0, 0, -50), true);
            Assert.That(Grid.HasStoredCell(0, 0), Is.True);
            Grid.SetCell(new Cell<int>(0, 0, -50), false);
            Assert.That(Grid.HasStoredCell(0, 0), Is.False);
        }

        [Test]
        public void RemoveStoredCell_ReturnsResult_Correct()
        {
            Assert.That(Grid.HasStoredCell(0, 0), Is.False);
            Grid.SetCell(new Cell<int>(0, 0, -50), true);
            Assert.That(Grid.HasStoredCell(0, 0), Is.True);
            Grid.RemoveStoredCell(0, 0);
            Assert.That(Grid.HasStoredCell(0, 0), Is.False);
        }
    }
}