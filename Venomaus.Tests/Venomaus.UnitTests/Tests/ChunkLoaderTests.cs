using System;
using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Chunking;
using Venomaus.FlowVitae.Chunking.Generators;
using Venomaus.FlowVitae.Grids;
using Venomaus.FlowVitae.Helpers;
using Venomaus.UnitTests.Tools;
using Direction = Venomaus.FlowVitae.Helpers.Direction;

namespace Venomaus.UnitTests.Tests
{
    [TestFixture(25, 25, 25, 25)]
    [TestFixture(50, 50, 25, 25)]
    [TestFixture(80, 35, 80, 35)]
    [TestFixture(69, 29, 13, 12)]
    [TestFixture(50, 50, 100, 100)]
    [TestOf(typeof(ChunkLoader<int, Cell<int>, IChunkData>))]
    [Parallelizable(ParallelScope.Fixtures)]
    internal class ChunkLoaderTests : BaseTests<int, Cell<int>>
    {
        private const int Seed = 1000;
        protected override IProceduralGen<int, Cell<int>> ProcGen => new ProceduralGenerator<int, Cell<int>>(Seed, GenerateChunk);

        protected event EventHandler<int[]>? OnGenerateChunk;

        private void GenerateChunk(Random random, int[] chunk, int width, int height, (int x, int y) chunkCoordinate)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    chunk[y * width + x] = random.Next(0, 10);
                }
            }
            InvokeChunkGenerationEvent(this, chunk);
        }

        protected void InvokeChunkGenerationEvent(object sender, int[] chunk)
        {
            OnGenerateChunk?.Invoke(sender, chunk);
        }

        public ChunkLoaderTests(int viewPortWidth, int viewPortHeight, int chunkWidth, int chunkHeight)
        {
            ViewPortWidth = viewPortWidth;
            ViewPortHeight = viewPortHeight;
            ChunkWidth = chunkWidth;
            ChunkHeight = chunkHeight;
        }

        [Test]
        public void GetChunksToLoad_Contains_CenterChunk()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var chunksToLoad = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y);
            var centerChunk = chunkLoader.GetChunkCoordinate(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y);
            Assert.That(chunksToLoad.AllChunks, Contains.Item(centerChunk));
        }

        [Test]
        public void GetChunksToLoad_Contains_AllChunksForViewPort()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var comparer = new TupleComparer<int>();
            var chunksToLoad = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y);
            var viewport = grid.GetViewPortWorldCoordinates()
                .Select(a => grid.GetChunkCoordinate(a.x, a.y))
                .ToHashSet(comparer);
            Assert.That(viewport.All(a => chunksToLoad.AllChunks.Contains(a, comparer)));
        }

        [Test]
        public void ChunkLoader_Is_Not_Null()
        {
            var grid = CreateNewGrid();
            var chunkLoader = () => grid._chunkLoader ?? throw new Exception("No chunkloader available");
            Assert.Multiple(() =>
            {
                Assert.That(chunkLoader.Invoke(), Is.Not.Null);
                Assert.That(() => chunkLoader.Invoke(), Throws.Nothing);
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
                Assert.That(() => new Grid<int, Cell<int>>(100, 100, 50, 50, ProcGen), Throws.Nothing);
                Assert.That(() => new Grid<int, Cell<int>>(100, 100, ProcGen), Throws.Nothing);
            });
        }

        [Test]
        public void StoreState_SetAndGet_Correct()
        {
            var grid = CreateNewGrid();

            int posX = ViewPortWidth / 2;
            int posY = ViewPortHeight / 2;

            // Check if original cell is not 4
            var cell = grid.GetCell(posX, posY);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.Walkable, Is.EqualTo(true));

            // Change cell to 4 with store state
            grid.SetCell(new Cell<int>(posX, posY, false, -10), true);
            Assert.That(() => grid.SetCell(null), Throws.Nothing);

            // Verify if cell is 4 and number matches stored state
            cell = grid.GetCell(posX, posY);
            Assert.That(cell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(cell.CellType, Is.EqualTo(-10));
                Assert.That(cell.Walkable, Is.EqualTo(false));
            });

            // Set cell to 1 with no store state
            grid.SetCell(posX, posY, -5, false);

            // Verify if cell is 1 and number is default again
            cell = grid.GetCell(posX, posY);
            Assert.That(cell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(cell.CellType, Is.EqualTo(-5));
                Assert.That(cell.Walkable, Is.Not.EqualTo(false));
            });
        }

        [Test]
        public void PositiveCoordinate_SetAndGet_Correct()
        {
            var grid = CreateNewGrid();
            // When not saving state in an unloaded chunk, the chunk data is lost
            grid.SetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, -5);
            var cell = grid.GetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.Not.EqualTo(-5));

            // When saving state in an unloaded chunk, the chunk data is stored
            grid.SetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, -5, true);
            cell = grid.GetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(-5));
        }

        [Test]
        public void NegativeCoordinate_SetAndGet_Correct()
        {
            var grid = CreateNewGrid();
            // When not saving state in an unloaded chunk, the chunk data is lost
            grid.SetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, -5);
            var cell = grid.GetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.Not.EqualTo(-5));

            var cellType = grid.GetCellType(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.That(cellType, Is.Not.EqualTo(-5));

            // When saving state in an unloaded chunk, the chunk data is stored
            grid.SetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, -5, true);
            cell = grid.GetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(-5));

            cellType = grid.GetCellType(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.That(cellType, Is.EqualTo(-5));
        }

        [Test]
        public void CurrentChunk_GetAndSet_Correct()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var current = chunkLoader.CurrentChunk;
            var chunkCoordinate = chunkLoader.GetChunkCoordinate(grid.Width / 2, grid.Height / 2);
            Assert.That(current.x, Is.EqualTo(chunkCoordinate.x));
            Assert.That(current.y, Is.EqualTo(chunkCoordinate.y));

            grid.Center(ViewPortWidth + ChunkWidth, ViewPortHeight + ChunkHeight);

            current = chunkLoader.CurrentChunk;
            Assert.That(current.x, Is.Not.EqualTo(chunkCoordinate.x));
            Assert.That(current.y, Is.Not.EqualTo(chunkCoordinate.y));
        }

        [Test]
        public void WorldCoordinateToChunkCoordinate_Remapping_Correct()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var screenBaseCoord = (x: 0, y: 0);

            // Current chunk
            var remappedCoordinateOnChunk = chunkLoader.RemapChunkCoordinate(5, 5);
            Assert.That(remappedCoordinateOnChunk.x, Is.EqualTo(screenBaseCoord.x + 5));
            Assert.That(remappedCoordinateOnChunk.y, Is.EqualTo(screenBaseCoord.y + 5));

            // Positive coord in another chunk
            remappedCoordinateOnChunk = chunkLoader.RemapChunkCoordinate(ChunkWidth + 5, ChunkHeight + 5);
            Assert.That(remappedCoordinateOnChunk.x, Is.EqualTo(screenBaseCoord.x + 5));
            Assert.That(remappedCoordinateOnChunk.y, Is.EqualTo(screenBaseCoord.y + 5));

            // Negative coord
            remappedCoordinateOnChunk = chunkLoader.RemapChunkCoordinate(-5, -5);
            Assert.That(remappedCoordinateOnChunk.x, Is.EqualTo(ChunkWidth + -5));
            Assert.That(remappedCoordinateOnChunk.y, Is.EqualTo(ChunkHeight + -5));
        }

        [Test]
        public void SetCurrentChunk_Correct()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var chunkCoordinate = chunkLoader.GetChunkCoordinate(grid.Width / 2, grid.Height / 2);
            Assert.That(chunkLoader.CurrentChunk.x, Is.EqualTo(chunkCoordinate.x));
            Assert.That(chunkLoader.CurrentChunk.y, Is.EqualTo(chunkCoordinate.y));

            chunkLoader.SetCurrentChunk(250, 250);
            chunkCoordinate = chunkLoader.GetChunkCoordinate(250, 250);
            Assert.That(chunkLoader.CurrentChunk.x, Is.EqualTo(chunkCoordinate.x));
            Assert.That(chunkLoader.CurrentChunk.y, Is.EqualTo(chunkCoordinate.y));

            chunkLoader.SetCurrentChunk(-250, -250);
            chunkCoordinate = chunkLoader.GetChunkCoordinate(-250, -250);
            Assert.That(chunkLoader.CurrentChunk.x, Is.EqualTo(chunkCoordinate.x));
            Assert.That(chunkLoader.CurrentChunk.y, Is.EqualTo(chunkCoordinate.y));
        }

        [Test]
        public void GetChunkCell_Get_Correct()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var posX = chunkLoader.CurrentChunk.x + ChunkWidth / 2;
            var posY = chunkLoader.CurrentChunk.y + ChunkHeight / 2;

            Cell<int>? cell = null;
            Assert.That(() => cell = chunkLoader.GetChunkCell(posX, posY), Throws.Nothing);
            Assert.That(() => chunkLoader.GetChunkCellType(posX, posY), Throws.Nothing);
            Assert.That(cell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(cell.X, Is.EqualTo(posX));
                Assert.That(cell.Y, Is.EqualTo(posY));
            });

            cell = null;
            Assert.Multiple(() =>
            {
                Assert.That(() => cell = chunkLoader.GetChunkCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5), Throws.Exception);
                Assert.That(() => chunkLoader.GetChunkCellType(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5), Throws.Exception);
                Assert.That(() => cell = chunkLoader.GetChunkCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, true), Throws.Nothing);
                Assert.That(() => chunkLoader.GetChunkCellType(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, true), Throws.Nothing);
            });
            Assert.That(cell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(cell.X, Is.EqualTo(ViewPortWidth + ChunkWidth * 5));
                Assert.That(cell.Y, Is.EqualTo(ViewPortHeight + ChunkHeight * 5));
            });
        }

        [Test]
        public void GetChunkCells_Get_Correct()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var cellPositions = new[] { (5, 5), (3, 2), (4, 4) };
            var cells = chunkLoader.GetChunkCells(cellPositions).ToArray();
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
        public void SetChunkCell_Set_Correct()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            int posX = ViewPortWidth / 2;
            int posY = ViewPortHeight / 2;

            var cell = chunkLoader.GetChunkCell(posX, posY);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.Walkable, Is.EqualTo(true));

            chunkLoader.SetChunkCell(new Cell<int>(posX, posY, false, 1), false, null, grid.IsWorldCoordinateOnScreen, grid.ScreenCells);

            var changedCell = chunkLoader.GetChunkCell(posX, posY, false, grid.IsWorldCoordinateOnScreen, grid.ScreenCells);
            var changedCellType = chunkLoader.GetChunkCellType(posX, posY, false, grid.IsWorldCoordinateOnScreen, grid.ScreenCells);
            Assert.That(changedCell, Is.Not.Null);
            Assert.That(changedCellType, Is.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(changedCell.CellType, Is.EqualTo(1));
                Assert.That(() => chunkLoader.SetChunkCell(new Cell<int>(grid.Width + 5, grid.Height + 5, false, 1)), Throws.Nothing);
                Assert.That(() => chunkLoader.SetChunkCell(new Cell<int>(-grid.Width - 5, -grid.Height - 5, false, 1)), Throws.Nothing);
            });

            // Set chunk cell in an off-screen chunk but that is loaded
            if (grid.IsChunkLoaded(grid.Width + 1, grid.Height))
            {
                Assert.That(grid.IsChunkLoaded(grid.Width + 1, grid.Height));
                grid.SetCell(grid.Width + 1, grid.Height, -50, false);
                cell = chunkLoader.GetChunkCell(grid.Width + 1, grid.Height);
                var cellType = chunkLoader.GetChunkCellType(grid.Width + 1, grid.Height);
                Assert.That(cellType, Is.EqualTo(-50));
                Assert.That(cell.CellType, Is.EqualTo(-50));
            }
        }

        [Test]
        public void LoadChunk_Load_Correct()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var loadedChunks = chunkLoader.GetLoadedChunks();
            var (x, y) = chunkLoader.GetChunkCoordinate(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.Multiple(() =>
            {
                Assert.That(!loadedChunks.Any(loadedChunk => loadedChunk.x == x &&
                            loadedChunk.y == y), "(X,Y) chunk was available.");
                Assert.That(loadedChunks.Any(loadedChunk => loadedChunk.x == chunkLoader.CurrentChunk.x &&
                    loadedChunk.y == chunkLoader.CurrentChunk.y), "No base chunk available.");

                // Can't load already loaded chunk
                Assert.That(chunkLoader.LoadChunk(chunkLoader.CurrentChunk.x, chunkLoader.CurrentChunk.y, out _), Is.False, "Was able to load base chunk.");
                Assert.That(chunkLoader.LoadChunk(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, out _), Is.True, "Was not able to load new chunk.");
            });
            loadedChunks = chunkLoader.GetLoadedChunks();

            Assert.That(loadedChunks.Any(loadedChunk => loadedChunk.x == x &&
                            loadedChunk.y == y));
        }

        [Test]
        public void UnloadChunk_Unload_Correct()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            (int x, int y)[]? loadedChunks = null;
            var expectedChunks = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y);

            Assert.That(() => loadedChunks = chunkLoader.GetLoadedChunks(), Has.Length.EqualTo(expectedChunks.AllChunks.Count).After(2).Seconds.PollEvery(10).MilliSeconds);
            Assert.That(loadedChunks, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(loadedChunks.Any(loadedChunk => loadedChunk.x == chunkLoader.CurrentChunk.x &&
                            loadedChunk.y == chunkLoader.CurrentChunk.y), "No base chunk loaded");
                // Can't unload mandatory chunk
                Assert.That(chunkLoader.UnloadChunk(chunkLoader.CurrentChunk.x, chunkLoader.CurrentChunk.y), Is.False, "Could unload base chunk");
                // Force unload it
                Assert.That(chunkLoader.UnloadChunk(chunkLoader.CurrentChunk.x, chunkLoader.CurrentChunk.y, true), Is.True, "Could not force unload");
                // Load an arbitrary chunk
                Assert.That(chunkLoader.LoadChunk(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, out _), Is.True, "Could not load arbitrary chunk");
                // See if it can be non force unloaded
                Assert.That(chunkLoader.UnloadChunk(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5), Is.True, "Could not unload arbitrary chunk");
            });
            loadedChunks = chunkLoader.GetLoadedChunks();

            Assert.That(!loadedChunks.Any(loadedChunk => loadedChunk.x == chunkLoader.CurrentChunk.x &&
                            loadedChunk.y == chunkLoader.CurrentChunk.y), "Base chunk was still loaded.");

            // Unload all chunks
            foreach (var chunk in loadedChunks)
                chunkLoader.UnloadChunk(chunk.x, chunk.y, true);
            loadedChunks = chunkLoader.GetLoadedChunks();
            Assert.Multiple(() =>
            {
                Assert.That(loadedChunks, Has.Length.EqualTo(0));
                Assert.That(chunkLoader.UnloadChunk(0, 0), Is.False);
            });
        }

        [Test]
        public void GetChunkCoordinate_Get_Correct()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var (x, y) = chunkLoader.GetChunkCoordinate(ChunkWidth - 5, ChunkHeight - 5);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(0));
                Assert.That(y, Is.EqualTo(0));
            });
            (x, y) = chunkLoader.GetChunkCoordinate(ChunkWidth + 5, ChunkHeight + 5);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(ChunkWidth));
                Assert.That(y, Is.EqualTo(ChunkHeight));
            });
            (x, y) = chunkLoader.GetChunkCoordinate(-5, -5);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(-ChunkWidth));
                Assert.That(y, Is.EqualTo(-ChunkHeight));
            });
        }

        [Test]
        public void UpdateScreenCells_Correct()
        {
            var grid = CreateNewGrid();
            var coords = grid.GetViewPortWorldCoordinates();
            int total = 0;
            grid.OnCellUpdate += (sender, args) =>
            {
                total++;
            };
            grid.UpdateScreenCells();
            Assert.That(total, Is.EqualTo(0));

            grid.RaiseOnlyOnCellTypeChange = false;
            grid.UpdateScreenCells();
            Assert.That(total, Is.EqualTo(coords.Count()));
        }

        [Test]
        public void Center_NoThreading_Works()
        {
            var grid = CreateNewGrid();
            grid.UseThreading = false;
            Assert.That(grid.UseThreading, Is.EqualTo(false));
            Center_ViewPort_Correct();
        }

        [Test]
        public void Center_Viewport_UsesScreenCells()
        {
            var grid = CreateNewGrid();
            bool triggered = false;
            grid.UseThreading = false;
            grid.OnCellUpdate += (sender, args) => { triggered = true; };
            grid.Center(0, 0);
            grid.Center(1, 0);
            Assert.That(triggered, Is.True);
        }

        [Test]
        public void Center_ViewPort_Correct()
        {
            var grid = CreateNewGrid(); 
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var viewPort = grid.GetViewPortWorldCoordinates()
                .ToArray();
            var viewPortCells = grid.GetCells(viewPort);
            Assert.That(viewPortCells.Cast<Cell<int>>().All(cell => cell.CellType != -10), "Initial viewport is not correct");

            var loadedChunks = chunkLoader.GetLoadedChunks();
            foreach (var (x, y) in loadedChunks)
                chunkLoader.UnloadChunk(x, y, true);

            // Set cells beforehand
            var positions = new List<(int x, int y)>();
            int halfViewPortX = (ViewPortWidth / 2);
            int halfViewPortY = (ViewPortHeight / 2);
            for (int x = 100 - halfViewPortX; x <= 100 + halfViewPortX; x++)
            {
                for (int y = 100 - halfViewPortY; y <= 100 + halfViewPortY; y++)
                {
                    positions.Add((x, y));
                }
            }
            var cells = grid.GetCells(positions).ToArray();
            foreach (var cell in cells)
            {
                if (cell != null)
                    cell.CellType = -10;
            }
            grid.SetCells(cells, true);

            int moduloWidth = (100 % ChunkWidth);
            int moduloHeight = (100 % ChunkHeight);
            var baseChunk = (x: 100 - moduloWidth, y: 100 - moduloHeight);

            var cellsUpdated = new List<Cell<int>>();
            grid.OnCellUpdate += (sender, args) => { if (args?.Cell != null) cellsUpdated.Add(args.Cell); };
            loadedChunks = chunkLoader.GetLoadedChunks();
            Assert.Multiple(() =>
            {
                Assert.That(() => grid.Center(100, 100), Throws.Nothing, "Exception was thrown");
                Assert.That(chunkLoader.CurrentChunk.x, Is.EqualTo(baseChunk.x), "Current chunk x is not correct");
                Assert.That(chunkLoader.CurrentChunk.y, Is.EqualTo(baseChunk.y), "Current chunk y is not correct");
            });
            var chunksToBeLoaded = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y).AllChunks.Count;
            Assert.Multiple(() =>
            {
                Assert.That(() => loadedChunks = chunkLoader.GetLoadedChunks().OrderBy(a => a.x).ThenBy(a => a.y).ToArray(), Has.Length.EqualTo(chunksToBeLoaded).After(1).Seconds.PollEvery(1).MilliSeconds, $"Loaded chunks not equal to {chunksToBeLoaded}");
                Assert.That(cellsUpdated, Has.Count.EqualTo(viewPort.Length), "cellsUpdated not equal to viewport length");
            });

            // Check if view port matches now
            viewPort = grid.GetViewPortWorldCoordinates().ToArray();
            viewPortCells = grid.GetCells(viewPort);
            Assert.That(viewPortCells.Cast<Cell<int>>().All(cell => cell.CellType == -10), "Viewport cells don't match center changes");
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
                Assert.That(() => grid.ScreenToWorldCoordinate(-(grid.Width + 5), -(grid.Height + 5)), Throws.Exception);
            });
        }

        [Test]
        public void WorldToScreenPoint_Get_Correct()
        {
            var grid = CreateNewGrid();
            var worldPosX = 5;
            var worldPosY = 5;
            var (x, y) = grid.WorldToScreenCoordinate(worldPosX, worldPosY);
            (int x, int y) _screenPos1 = (0, 0), _screenPos2 = (0, 0), _screenPos3 = (0, 0);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(worldPosX));
                Assert.That(y, Is.EqualTo(worldPosY));
                Assert.That(() => _screenPos1 = grid.WorldToScreenCoordinate(grid.Width + 5, grid.Height + 5), Throws.Nothing);
                Assert.That(() => _screenPos2 = grid.WorldToScreenCoordinate(-5, -5), Throws.Nothing);
                Assert.That(() => _screenPos3 = grid.WorldToScreenCoordinate(-(grid.Width + 5), -(grid.Height + 5)), Throws.Nothing);
            });
            Assert.Multiple(() =>
            {
                Assert.That(_screenPos1.x, Is.EqualTo(grid.Width + 5));
                Assert.That(_screenPos1.y, Is.EqualTo(grid.Height + 5));
                Assert.That(_screenPos2.x, Is.EqualTo(-5));
                Assert.That(_screenPos2.y, Is.EqualTo(-5));
                Assert.That(_screenPos3.x, Is.EqualTo(-(grid.Width + 5)));
                Assert.That(_screenPos3.y, Is.EqualTo(-(grid.Height + 5)));
            });
        }

        [Test]
        public void ChunkLoading_CorrectDuring_Centering()
        {
            var grid = CreateNewGrid();

            // Test non-diagonal centering
            CenterTowards(grid, 0, 0, Direction.North);
            CenterTowards(grid, 0, 0, Direction.East);
            CenterTowards(grid, 0, 0, Direction.South);
            CenterTowards(grid, 0, 0, Direction.West);

            // Test diagonal centering
            CenterTowards(grid, 0, 0, Direction.NorthEast);
            CenterTowards(grid, 0, 0, Direction.NorthWest);
            CenterTowards(grid, 0, 0, Direction.SouthEast);
            CenterTowards(grid, 0, 0, Direction.SouthWest);
        }

        private void CenterTowards(Grid<int, Cell<int>> grid, int startX, int startY, Direction dir)
        {
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            // Start at 0, 0 => Go in all directions
            grid.Center(startX, startY);

            int dirX = 0, dirY = 0;
            switch (dir)
            {
                case Direction.North:
                    dirY = startY + ChunkHeight;
                    break;
                case Direction.East:
                    dirX = startX + ChunkWidth;
                    break;
                case Direction.South:
                    dirY = startY - ChunkHeight;
                    break;
                case Direction.West:
                    dirX = startX - ChunkWidth;
                    break;
                case Direction.NorthEast:
                    dirY = startY + ChunkHeight;
                    dirX = startX + ChunkWidth;
                    break;
                case Direction.NorthWest:
                    dirY = startY + ChunkHeight;
                    dirX = startX - ChunkWidth;
                    break;
                case Direction.SouthEast:
                    dirY = startY - ChunkHeight;
                    dirX = startX + ChunkWidth;
                    break;
                case Direction.SouthWest:
                    dirY = startY - ChunkHeight;
                    dirX = startX - ChunkWidth;
                    break;
            }

            (int x, int y)[] loadedChunks = Array.Empty<(int x, int y)>();
            var startCount = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y).AllChunks.Count;
            Assert.That(() => loadedChunks = chunkLoader.GetLoadedChunks(), Has.Length.EqualTo(startCount).After(1).Seconds.PollEvery(1).MilliSeconds);
            Assert.Multiple(() =>
            {
                Assert.That(loadedChunks.Any(a => a.x == 0 && a.y == 0), $"Base chunk not loaded! | Dir: {dir}");
                Assert.That(loadedChunks.Any(a => a.x == dirX && a.y == dirY), $"New direction chunk not loaded! | Dir: {dir}");
            });

            // Move upwards
            var sizeDir = dirX != 0 && dirY != 0 ? Math.Max(ChunkWidth, ChunkHeight) : dirX != 0 ? ChunkWidth : ChunkHeight;
            for (int i = 0; i < sizeDir; i++)
            {
                var xOffset = dirX != 0 ? dirX < 0 ? i > ChunkWidth ? -ChunkWidth : -i : i > ChunkWidth ? ChunkWidth : i : 0;
                var yOffset = dirY != 0 ? dirY < 0 ? i > ChunkHeight ? -ChunkHeight : -i : i > ChunkHeight ? ChunkHeight : i : 0;
                grid.Center(xOffset, yOffset);

                var updatedCount = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y).AllChunks.Count;
                Assert.That(() => loadedChunks = chunkLoader.GetLoadedChunks(), Has.Length.EqualTo(updatedCount).After(1).Seconds.PollEvery(1).MilliSeconds);
                Assert.Multiple(() =>
                {
                    Assert.That(loadedChunks.Any(a => a.x == 0 && a.y == 0), $"Chunk (0, 0) not loaded | Dir: {dir}");
                    Assert.That(loadedChunks.Any(a => a.x == dirX && a.y == dirY));
                });
            }

            grid.Center(dirX != 0 ? dirX : 0, dirY != 0 ? dirY : 0);

            var newUpdatedCount = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y).AllChunks.Count;
            Assert.That(() => loadedChunks = chunkLoader.GetLoadedChunks(), Has.Length.EqualTo(newUpdatedCount).After(1).Seconds.PollEvery(1).MilliSeconds);
            Assert.Multiple(() =>
            {
                Assert.That(loadedChunks.Any(a => a.x == 0 && a.y == 0), $"Chunk (0, 0) not loaded | Dir: {dir}");
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
        public void GetCell_ReturnsCorrectChunkScreenCell_IfScreenCell_WasNotYetAdjusted()
        {
            var grid = CreateNewGrid(Seed, (rand, chunk, width, height, chunkCoordinate) =>
            {
                // Set default to -5 on all cells
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        chunk[y * width + x] = -5;
                    }
                }
            });

            grid.Center(0, 0);
            Assert.That(grid._chunkLoader, Is.Not.Null);
            var expectedLoadedChunks = grid._chunkLoader.GetChunksToLoad(0, 0);
            Assert.That(() => grid._chunkLoader.GetLoadedChunks(), Has.Length.EqualTo(expectedLoadedChunks.AllChunks.Count).After(1).Seconds.PollEvery(10).MilliSeconds);

            // Verify that all cells have this default value set properly
            var viewPort = grid.GetViewPortWorldCoordinates();
            var viewPortCells = grid.GetCells(viewPort).ToArray();
            Assert.That(viewPortCells.Cast<Cell<int>>().Count(a => a.CellType == -5), Is.EqualTo(viewPortCells.Length),
                string.Join(",", viewPortCells.Cast<Cell<int>>().GroupBy(a => a.CellType).Select(a => a.Key)));

            // Verify that GetCell has this default value set properly
            var cell = grid.GetCell(0, 0);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(-5));

            // Verify that GetCells has this default value set properly
            var cells = grid.GetCells(new[] { (0, 0) });
            cell = cells.SingleOrDefault();
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(-5));

            grid.SetCell(0, 0, 5);
            viewPort = grid.GetViewPortWorldCoordinates(a => a == 5);
            Assert.That(viewPort.ToArray(), Has.Length.EqualTo(1));
        }

        [Test]
        public void SetZeroChunkSize_Throws_Exception()
        {
            Grid<int, Cell<int>>? newGrid = null;
            Assert.Multiple(() =>
            {
                Assert.That(() => newGrid = new Grid<int, Cell<int>>(100, 100, 0, 0, ProcGen), Throws.Exception);
                Assert.That(newGrid, Is.Null);
            });
        }

        [Test]
        public virtual void GenerateChunk_ChunkData_IsAlwaysSame()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            int chunkX = ViewPortWidth + ChunkWidth * 10;
            int chunkY = ViewPortHeight + ChunkHeight * 10;
            var chunkCoords = chunkLoader.GetChunkCoordinate(chunkX, chunkY);

            Assert.That(() => chunkLoader.LoadChunk(chunkX, chunkY, out _), Is.True);

            // Collect current chunk data
            int[] chunkData = new int[ChunkWidth * ChunkHeight];
            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int y = 0; y < ChunkHeight; y++)
                {
                    var cell = grid.GetCell(chunkCoords.x + x, chunkCoords.y + y);
                    Assert.That(cell, Is.Not.Null);
                    chunkData[y * ChunkWidth + x] = cell.CellType;
                }
            }

            Assert.That(() => chunkLoader.UnloadChunk(chunkX, chunkY), Is.True, "Chunk not unloaded.");

            bool eventRaised = false;
            void genCheck(object? sender, int[] chunk)
            {
                Assert.That(chunkData.SequenceEqual(chunk));
                eventRaised = true;
            }
            OnGenerateChunk += genCheck;
            Assert.Multiple(() =>
            {
                Assert.That(() => chunkLoader.LoadChunk(chunkX, chunkY, out _), Is.True);
                Assert.That(eventRaised, Is.True);
            });
            OnGenerateChunk -= genCheck;
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
                    cells.Add(new Cell<int>(x, y, -10));
                }
            }

            List<Cell<int>?> prevState = grid.GetCells(cells.Where(a => a != null).Cast<Cell<int>>().Select(a => (a.X, a.Y))).ToList();
            grid.SetCells(cells, true);

            Assert.That(() => grid.ClearCache(), Throws.Nothing);

            cells = grid.GetCells(cells.Where(a => a != null).Cast<Cell<int>>().Select(a => (a.X, a.Y))).ToList();

            Assert.That(cells.SequenceEqual(prevState, new CellFullComparer<int>()), "Cells are not reset.");
        }

        [Test]
        public void GetChunkData_ChunkedGrid_WithNoDataProvided_ReturnsNull()
        {
            var grid = CreateNewGrid();
            Assert.That(grid.GetChunkData(0, 0), Is.Null);
        }

        [Test]
        public virtual void GetChunkData_Returns_ValidData()
        {
            // Custom chunk generation implementation
            Func<Random, int[], int, int, (int x, int y), TestChunkData> chunkGenerationMethod = (random, chunk, width, height, chunkCoordinate) =>
            {
                // Define custom chunk data
                var chunkData = new TestChunkData
                {
                    Trees = new HashSet<(int x, int y)>(new TupleComparer<int>())
                };
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        // Every chunk will have a tree at 0, 0
                        if (x == 0 && y == 0)
                            chunkData.Trees.Add((x, y));
                        chunk[y * width + x] = random.Next(-10, 10);
                    }
                }
                return chunkData;
            };

            // Initialize the custom implementations
            var customProcGen = new ProceduralGenerator<int, Cell<int>, TestChunkData>(Seed, chunkGenerationMethod);
            var customGrid = new Grid<int, Cell<int>, TestChunkData>(ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight, customProcGen);

            Assert.That(customGrid._chunkLoader, Is.Not.Null);

            // Chunk data retrieval
            TestChunkData? customChunkData = null;
            Assert.That(() => customChunkData = customGrid.GetChunkData(0, 0), Throws.Nothing);

            // Chunk data verification
            Assert.That(customChunkData, Is.Not.Null);
            Assert.That(customChunkData.Trees, Is.Not.Null);
            Assert.That(customChunkData.Trees, Has.Count.EqualTo(1));
            Assert.That(customChunkData.Trees.First(), Is.EqualTo((0, 0)));

            // Check if seed matches
            var chunkCoordinate = customGrid._chunkLoader.GetChunkCoordinate(0, 0);
            var seed = Fnv1a.Hash32(chunkCoordinate.x, chunkCoordinate.y, Seed);
            Assert.That(customChunkData.Seed, Is.EqualTo(seed));

            // Attempt to store chunk data with some different data
            customChunkData.Trees.Add((5, 5));
            customGrid.StoreChunkData(customChunkData);
            // Check double save works fine
            Assert.That(() => customGrid.StoreChunkData(customChunkData), Throws.Nothing);
            // Reload chunk data
            customChunkData = customGrid.GetChunkData(0, 0);
            Assert.That(customChunkData, Is.Not.Null);
            Assert.That(customChunkData.Trees, Contains.Item((5, 5)));

            // Reload chunk
            customGrid._chunkLoader.UnloadChunk(0, 0, true);
            customGrid._chunkLoader.LoadChunk(0, 0, out _);

            customChunkData = customGrid.GetChunkData(0, 0);
            Assert.That(customChunkData, Is.Not.Null);
            Assert.That(customChunkData.Trees, Contains.Item((5, 5)));

            customGrid.RemoveChunkData(customChunkData, true);

            // Reload chunk data
            customChunkData = customGrid.GetChunkData(0, 0);
            Assert.That(customChunkData, Is.Not.Null);

            // Default checks again if it matches still the generated chunk data
            Assert.That(customChunkData.Trees, Is.Not.Null);
            Assert.That(customChunkData.Trees, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(customChunkData.Trees.First(), Is.EqualTo((0, 0)));
                // Check if seed matches
                Assert.That(customChunkData.Seed, Is.EqualTo(seed));
            });
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
        public void ChunkEvents_NoThreading_Raised_Properly()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var chunksLoaded = new List<(int x, int y)>();
            var chunksUnloaded = new List<(int x, int y)>();

            var initialChunks = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y);
            Assert.That(() => grid.GetLoadedChunkCoordinates().ToArray(), Has.Length.EqualTo(initialChunks.AllChunks.Count).After(2).Seconds.PollEvery(10).MilliSeconds);
            void ChunkLoaded(object? sender, ChunkUpdateArgs args)
            {
                chunksLoaded.Add((args.ChunkX, args.ChunkY));
            }
            void ChunkUnloaded(object? sender, ChunkUpdateArgs args)
            {
                chunksUnloaded.Add((args.ChunkX, args.ChunkY));
            }
            grid.UseThreading = false;
            grid.OnChunkLoad += ChunkLoaded;
            grid.OnChunkUnload += ChunkUnloaded;
            grid.Center(ViewPortWidth / 2 + ChunkWidth, ViewPortHeight / 2);

            var (x, y) = chunkLoader.GetChunkCoordinate(ViewPortWidth / 2, ViewPortHeight / 2);
            int baseWidth = x;
            int baseHeight = y;

            var newChunks = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y);
            var differenceChunks = newChunks.GetDifference(initialChunks);

            var (addedChunks, removedChunks) = ChunkLoadInformation.GetAddedRemovedChunks(initialChunks, newChunks);
            var comparer = new TupleComparer<int>();

            Assert.Multiple(() =>
            {
                Assert.That(chunksLoaded, Has.Count.EqualTo(differenceChunks.AllChunks.Count), "Loaded: " + chunksLoaded.Count + " | " + string.Join(", ", chunksLoaded));
                Assert.That(chunksUnloaded, Has.Count.EqualTo(differenceChunks.AllChunks.Count), "Unloaded: " + chunksUnloaded.Count + " | " + string.Join(", ", chunksUnloaded));
                Assert.That(chunksLoaded.OrderBy(a => a.x).ThenBy(a => a.y).SequenceEqual(addedChunks, comparer), "Loaded chunks was incorrect");
                Assert.That(chunksUnloaded.OrderBy(a => a.x).ThenBy(a => a.y).SequenceEqual(removedChunks, comparer), "Unloaded chunks was incorrect");
            });
            grid.OnChunkLoad -= ChunkLoaded;
            grid.OnChunkUnload -= ChunkUnloaded;
        }

        [Test]
        public void ChunkEvents_Threading_Raised_Properly()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var currentLoadedChunks = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y);

            Assert.That(() => chunkLoader.GetLoadedChunks(), Has.Length.EqualTo(currentLoadedChunks.AllChunks.Count).After(2).Seconds.PollEvery(10).MilliSeconds);

            var chunksLoaded = new List<(int x, int y)>();
            var chunksUnloaded = new List<(int x, int y)>();
            var comparer = new TupleComparer<int>();

            void ChunkLoaded(object? sender, ChunkUpdateArgs args)
            {
                chunksLoaded.Add((args.ChunkX, args.ChunkY));

                var positions = args.GetCellPositions().ToArray();
                Assert.That(positions, Has.Length.EqualTo(ChunkWidth * ChunkHeight));
                foreach (var pos in positions)
                    Assert.That(comparer.Equals(chunkLoader.GetChunkCoordinate(pos.x, pos.y), (args.ChunkX, args.ChunkY)));
            }
            void ChunkUnloaded(object? sender, ChunkUpdateArgs args)
            {
                chunksUnloaded.Add((args.ChunkX, args.ChunkY));

                var positions = args.GetCellPositions().ToArray();
                Assert.That(positions, Has.Length.EqualTo(ChunkWidth * ChunkHeight));
                foreach (var pos in positions)
                    Assert.That(comparer.Equals(chunkLoader.GetChunkCoordinate(pos.x, pos.y), (args.ChunkX, args.ChunkY)));
            }

            grid.OnChunkLoad += ChunkLoaded;
            grid.OnChunkUnload += ChunkUnloaded;

            grid.UseThreading = true;
            grid.Center(ViewPortWidth / 2 + ChunkWidth, ViewPortHeight / 2);

            var expectedChunksLoaded = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y);
            var differenceBetweenChunksLoaded = expectedChunksLoaded.GetDifference(currentLoadedChunks);

            var newChunks = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y);
            var differenceChunks = newChunks.GetDifference(currentLoadedChunks);

            var (addedChunks, removedChunks) = ChunkLoadInformation.GetAddedRemovedChunks(currentLoadedChunks, newChunks);

            Assert.Multiple(() =>
            {
                Assert.That(() => chunksLoaded, Has.Count.EqualTo(differenceBetweenChunksLoaded.ChunksOutsideViewport.Count).After(2).Seconds.PollEvery(10).MilliSeconds, "Times load incorrect");
                Assert.That(() => chunksUnloaded, Has.Count.EqualTo(differenceBetweenChunksLoaded.ChunksOutsideViewport.Count).After(2).Seconds.PollEvery(10).MilliSeconds, "Times unload incorrect");
                Assert.That(() => chunksLoaded.OrderBy(a => a.x).ThenBy(a => a.y).SequenceEqual(addedChunks.OrderBy(a => a.x).ThenBy(a => a.y), comparer), Is.True.After(2).Seconds.PollEvery(10).MilliSeconds, "Loaded chunks was incorrect");
                Assert.That(() => chunksUnloaded.OrderBy(a => a.x).ThenBy(a => a.y).SequenceEqual(removedChunks.OrderBy(a => a.x).ThenBy(a => a.y), comparer), Is.True.After(2).Seconds.PollEvery(10).MilliSeconds, "Unloaded chunks was incorrect");
            });

            grid.OnChunkLoad -= ChunkLoaded;
            grid.OnChunkUnload -= ChunkUnloaded;
        }

        [Test]
        public void GetChunkSeed_Returns_CorrectValue()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var chunkSeed = grid.GetChunkSeed(0, 0);
            var chunkCoordinate = chunkLoader.GetChunkCoordinate(0, 0);
            var seed = Fnv1a.Hash32(chunkCoordinate.x, chunkCoordinate.y, ProcGen.Seed);
            Assert.That(chunkSeed, Is.EqualTo(seed));
        }

        [Test]
        public void IsChunkLoaded_Returns_CorrectValue()
        {
            var grid = CreateNewGrid();
            var chunkLoaded = grid.IsChunkLoaded(ViewPortWidth / 2, ViewPortHeight / 2);
            Assert.That(chunkLoaded, Is.True);
        }

        [Test]
        public void InBounds_Returns_CorrectValue()
        {
            var grid = CreateNewGrid();
            var chunkLoaded = grid.InBounds(ViewPortWidth + ChunkWidth * 10, ViewPortHeight + ChunkHeight * 10);
            Assert.That(chunkLoaded, Is.True);
            Assert.That(grid.InBounds(null), Is.False);
        }

        [Test]
        public void ClearCache_RaisesChunkEvents_Correctly()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var expectedChunkInfo = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y);

            Assert.That(() => chunkLoader.GetLoadedChunks(), Has.Length.EqualTo(expectedChunkInfo.AllChunks.Count).After(2).Seconds.PollEvery(10).MilliSeconds);

            int loaded = 0, unloaded = 0;
            void ChunkLoaded(object? sender, EventArgs args) 
            {
                loaded++;
            }
            void ChunkUnloaded(object? sender, EventArgs args)
            {
                unloaded++;
            }
            grid.OnChunkLoad += ChunkLoaded;
            grid.OnChunkUnload += ChunkUnloaded;
            grid.ClearCache();

            Assert.That(() => unloaded, Is.EqualTo(expectedChunkInfo.AllChunks.Count).After(2).Seconds.PollEvery(10).MilliSeconds, "Unloaded chunks not correct");
            Assert.That(() => loaded, Is.EqualTo(expectedChunkInfo.AllChunks.Count).After(2).Seconds.PollEvery(10).MilliSeconds, "Loaded chunks not correct");
            grid.OnChunkLoad -= ChunkLoaded;
            grid.OnChunkUnload -= ChunkUnloaded;
        }

        [Test]
        public void GetChunkCoordinate_ReturnsResult_Correct()
        {
            var grid = CreateNewGrid();
            var comparer = new TupleComparer<int>();
            var coord = grid.GetChunkCoordinate(ChunkWidth / 2, ChunkHeight / 2);
            Assert.That(comparer.Equals(coord, (0,0)));
        }

        [Test]
        public void GetLoadedChunkCoordinates_ReturnsResult_Correct()
        {
            var grid = CreateNewGrid();
            var chunkLoader = grid._chunkLoader ?? throw new Exception("No chunkloader available");
            var loadedChunks = grid.GetLoadedChunkCoordinates();
            var expectedLoadedChunks = chunkLoader.GetChunksToLoad(chunkLoader.CenterCoordinate.x, chunkLoader.CenterCoordinate.y);
            Assert.That(() => grid.GetLoadedChunkCoordinates().ToArray(), Has.Length.EqualTo(expectedLoadedChunks.AllChunks.Count).After(2).Seconds.PollEvery(10).MilliSeconds);
        }

        [Test]
        public void GetChunkCellCoordinates_ReturnsResult_Correct()
        {
            var grid = CreateNewGrid();
            var loadedChunks = grid.GetChunkCellCoordinates(0,0).ToHashSet();
            Assert.That(loadedChunks, Has.Count.EqualTo(ChunkWidth * ChunkHeight));
            for (var x = 0; x < ChunkWidth; x++)
            {
                for (var y = 0; y < ChunkHeight; y++)
                {
                    Assert.That(loadedChunks.Contains((x, y)));
                }
            }
        }

        [Test]
        public void HasStoredCell_ReturnsResult_Correct()
        {
            var grid = CreateNewGrid();
            Assert.That(grid.HasStoredCell(grid.ChunkWidth + 5, grid.ChunkHeight + 5), Is.False);
            grid.SetCell(new Cell<int>(grid.ChunkWidth + 5, grid.ChunkHeight + 5, -50), true);
            Assert.That(grid.HasStoredCell(grid.ChunkWidth + 5, grid.ChunkHeight + 5), Is.True);
            grid.SetCell(new Cell<int>(grid.ChunkWidth + 5, grid.ChunkHeight + 5, -50), false);
            Assert.That(grid.HasStoredCell(grid.ChunkWidth + 5, grid.ChunkHeight + 5), Is.False);
        }

        [Test]
        public void RemoveStoredCell_ReturnsResult_Correct()
        {
            var grid = CreateNewGrid();
            Assert.That(grid.HasStoredCell(grid.ChunkWidth + 5, grid.ChunkHeight + 5), Is.False);
            grid.SetCell(new Cell<int>(grid.ChunkWidth + 5, grid.ChunkHeight + 5, -50), true);
            Assert.That(grid.HasStoredCell(grid.ChunkWidth + 5, grid.ChunkHeight + 5), Is.True);
            grid.RemoveStoredCell(grid.ChunkWidth + 5, grid.ChunkHeight + 5);
            Assert.That(grid.HasStoredCell(grid.ChunkWidth + 5, grid.ChunkHeight + 5), Is.False);
        }

        [Test]
        public void GetCells_CanReturn_NullValues()
        {
            var grid = CreateNewGrid();
            grid.SetCustomConverter((x, y, cellType) =>
            {
                return cellType != -1 ? new Cell<int>(x, y, cellType) : null;
            });

            (int x, int y) pos = (grid.ChunkWidth / 2, grid.ChunkHeight / 2);
            grid.SetCell(pos.x, pos.y, -1);

            // Check for null
            var cell = grid.GetCell(pos.x, pos.y);
            Assert.That(cell, Is.Null);
            var cellType = grid.GetCellType(pos.x, pos.y);
            Assert.That(cellType, Is.EqualTo(-1));

            var cells = grid.GetCells(new[] { pos }).ToArray();
            Assert.That(cells, Is.Not.Null);
            Assert.That(cells, Has.Length.EqualTo(1));
            Assert.That(cells[0], Is.Null);

            // Check for not null
            cell = grid.GetCell(pos.x + 1, pos.y + 1);
            Assert.That(cell, Is.Not.Null);

            // With storing state
            grid.SetCell(pos.x + 2, pos.y + 2, -1, true);
            cell = grid.GetCell(pos.x + 2, pos.y + 2);
            Assert.That(cell, Is.Not.Null);
            cellType = grid.GetCellType(pos.x, pos.y);
            Assert.That(cellType, Is.EqualTo(-1));

            cells = grid.GetCells(new[] { (pos.x + 1, pos.y + 1) }).ToArray();
            Assert.That(cells, Is.Not.Null);
            Assert.That(cells, Has.Length.EqualTo(1));
            Assert.That(cells[0], Is.Not.Null);
        }

        [Test]
        public void GetViewPortCells_Get_Correct()
        {
            var grid = CreateNewGrid();
            var viewPort = grid.GetViewPortWorldCoordinates().ToArray();
            Assert.That(viewPort, Has.Length.EqualTo(grid.Width * grid.Height));

            // Check if the order is also correct
            var comparer = new TupleComparer<int>();
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var index = y * grid.Width + x;
                    Assert.That(comparer.Equals(viewPort[index], (x, y)));
                }
            }
        }

        [Test]
        public void Exceptions_WhenChunkGenerator_RetrievesInvalidChunks()
        {
            var nullCellProcGen = new CustomGenerator(ProcGen.Seed, CustomGenerator.ReturnValue.NullCells);
            Assert.That(() => new Grid<int, Cell<int>>(ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight, nullCellProcGen), Throws.Exception);
            var invalidSizeProcGen = new CustomGenerator(ProcGen.Seed, CustomGenerator.ReturnValue.InvalidChunkSize);
            Assert.That(() => new Grid<int, Cell<int>>(ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight, invalidSizeProcGen), Throws.Exception);
        }

        class CustomGenerator : IProceduralGen<int, Cell<int>>
        {
            public int Seed { get; }

            public enum ReturnValue
            {
                NullCells,
                InvalidChunkSize
            }

            private readonly ReturnValue _returnValue;

            public CustomGenerator(int seed, ReturnValue returnValue)
            {
                Seed = seed;
                _returnValue = returnValue;
            }

            public (int[] chunkCells, IChunkData? chunkData) Generate(int seed, int width, int height, (int x, int y) chunkCoordinate)
            {
                if (_returnValue == ReturnValue.NullCells)
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                    return (null, null);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
                else
                    return (new int[width / 2 * (height / 2)], null);
            }
        }
    }
}
