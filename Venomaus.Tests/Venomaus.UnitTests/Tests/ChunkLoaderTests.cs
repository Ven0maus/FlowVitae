using System.Linq;
using Venomaus.FlowVitae.Basics;
using Venomaus.FlowVitae.Basics.Chunking;
using Venomaus.FlowVitae.Basics.Procedural;
using Venomaus.FlowVitae.Cells;
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
    [TestOf(typeof(ChunkLoader<int, Cell<int>>))]
    internal class ChunkLoaderTests : BaseTests<int, Cell<int>>
    {
        private const int Seed = 1000;
        protected override IProceduralGen<int, Cell<int>>? ProcGen => new ProceduralGenerator<int, Cell<int>>(Seed, GenerateChunk);

        private event EventHandler<int[]>? onGenerateChunk;

        private void GenerateChunk(Random random, int[] chunk, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    chunk[y * width + x] = random.Next(0, 10);
                }
            }
            onGenerateChunk?.Invoke(this, chunk);
        }

        public ChunkLoaderTests(int viewPortWidth, int viewPortHeight, int chunkWidth, int chunkHeight)
        {
            ViewPortWidth = viewPortWidth;
            ViewPortHeight = viewPortHeight;
            ChunkWidth = chunkWidth;
            ChunkHeight = chunkHeight;
        }

        [Test]
        public void ChunkLoader_Is_Not_Null()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ChunkLoader, Is.Not.Null);
                Assert.That(() => ChunkLoader, Throws.Nothing);
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
            int posX = ChunkLoader.CurrentChunk.x + (ChunkWidth / 2);
            int posY = ChunkLoader.CurrentChunk.y + (ChunkHeight / 2);

            // Check if original cell is not 4
            var cell = Grid.GetCell(posX, posY);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.Walkable, Is.EqualTo(true));

            // Change cell to 4 with store state
            Grid.SetCell(new Cell<int>(posX, posY, false, -10), true);

            // Verify if cell is 4 and number matches stored state
            cell = Grid.GetCell(posX, posY);
            Assert.That(cell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(cell.CellType, Is.EqualTo(-10));
                Assert.That(cell.Walkable, Is.EqualTo(false));
            });

            // Set cell to 1 with no store state
            Grid.SetCell(posX, posY, -5, false);

            // Verify if cell is 1 and number is default again
            cell = Grid.GetCell(posX, posY);
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
            // When not saving state in an unloaded chunk, the chunk data is lost
            Grid.SetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, -5);
            var cell = Grid.GetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.Not.EqualTo(-5));

            // When saving state in an unloaded chunk, the chunk data is stored
            Grid.SetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, -5, true);
            cell = Grid.GetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(-5));
        }

        [Test]
        public void NegativeCoordinate_SetAndGet_Correct()
        {
            // When not saving state in an unloaded chunk, the chunk data is lost
            Grid.SetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, -5);
            var cell = Grid.GetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.Not.EqualTo(-5));

            var cellType = Grid.GetCellType(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.That(cellType, Is.Not.EqualTo(-5));

            // When saving state in an unloaded chunk, the chunk data is stored
            Grid.SetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, -5, true);
            cell = Grid.GetCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(-5));

            cellType = Grid.GetCellType(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.That(cellType, Is.EqualTo(-5));
        }

        [Test]
        public void CurrentChunk_GetAndSet_Correct()
        {
            var current = ChunkLoader.CurrentChunk;
            var chunkCoordinate = ChunkLoader.GetChunkCoordinate(Grid.Width / 2, Grid.Height /2);
            Assert.That(current.x, Is.EqualTo(chunkCoordinate.x));
            Assert.That(current.y, Is.EqualTo(chunkCoordinate.y));

            Grid.Center(ViewPortWidth + 10, ViewPortHeight + 10);

            current = ChunkLoader.CurrentChunk;
            Assert.That(current.x, Is.Not.EqualTo(chunkCoordinate.x));
            Assert.That(current.y, Is.Not.EqualTo(chunkCoordinate.y));

            chunkCoordinate = ChunkLoader.GetChunkCoordinate(ViewPortWidth + 10, ViewPortHeight + 10);
            Assert.That(current.x, Is.EqualTo(chunkCoordinate.x));
            Assert.That(current.y, Is.EqualTo(chunkCoordinate.y));
        }

        [Test]
        public void WorldCoordinateToChunkCoordinate_Remapping_Correct()
        {
            var screenBaseCoord = (x: 0, y: 0);

            // Current chunk
            var remappedCoordinateOnChunk = ChunkLoader.RemapChunkCoordinate(5, 5);
            Assert.That(remappedCoordinateOnChunk.x, Is.EqualTo(screenBaseCoord.x + 5));
            Assert.That(remappedCoordinateOnChunk.y, Is.EqualTo(screenBaseCoord.y + 5));

            // Positive coord in another chunk
            remappedCoordinateOnChunk = ChunkLoader.RemapChunkCoordinate(ChunkWidth + 5, ChunkHeight + 5);
            Assert.That(remappedCoordinateOnChunk.x, Is.EqualTo(screenBaseCoord.x + 5));
            Assert.That(remappedCoordinateOnChunk.y, Is.EqualTo(screenBaseCoord.y + 5));

            // Negative coord
            remappedCoordinateOnChunk = ChunkLoader.RemapChunkCoordinate(-5, -5);
            Assert.That(remappedCoordinateOnChunk.x, Is.EqualTo(ChunkWidth + -5));
            Assert.That(remappedCoordinateOnChunk.y, Is.EqualTo(ChunkHeight + -5));
        }

        [Test]
        public void SetCurrentChunk_Correct()
        {
            var chunkCoordinate = ChunkLoader.GetChunkCoordinate(Grid.Width / 2, Grid.Height / 2);
            Assert.That(ChunkLoader.CurrentChunk.x, Is.EqualTo(chunkCoordinate.x));
            Assert.That(ChunkLoader.CurrentChunk.y, Is.EqualTo(chunkCoordinate.y));

            ChunkLoader.SetCurrentChunk(250, 250);
            chunkCoordinate = ChunkLoader.GetChunkCoordinate(250, 250);
            Assert.That(ChunkLoader.CurrentChunk.x, Is.EqualTo(chunkCoordinate.x));
            Assert.That(ChunkLoader.CurrentChunk.y, Is.EqualTo(chunkCoordinate.y));

            ChunkLoader.SetCurrentChunk(-250, -250);
            chunkCoordinate = ChunkLoader.GetChunkCoordinate(-250, -250);
            Assert.That(ChunkLoader.CurrentChunk.x, Is.EqualTo(chunkCoordinate.x));
            Assert.That(ChunkLoader.CurrentChunk.y, Is.EqualTo(chunkCoordinate.y));
        }

        [Test]
        public void GetNeighborChunk_AllDirections_Correct()
        {
            Grid.Center(0, 0);

            int moduloWidth = (0 % ChunkWidth);
            int moduloHeight = (0 % ChunkHeight);
            var baseChunk = (x: 0 - moduloWidth, y: 0 - moduloHeight);

            // All mandatory chunks
            var mapping = new[]
            {
                (baseChunk.x - ChunkWidth, baseChunk.y - ChunkHeight),
                (baseChunk.x - ChunkWidth, baseChunk.y),
                (baseChunk.x - ChunkWidth, baseChunk.y + ChunkHeight),
                (baseChunk.x, baseChunk.y - ChunkHeight),
                (baseChunk.x, baseChunk.y + ChunkHeight),
                (baseChunk.x + ChunkWidth, baseChunk.y - ChunkHeight),
                (baseChunk.x + ChunkWidth, baseChunk.y),
                (baseChunk.x + ChunkWidth, baseChunk.y + ChunkHeight)
            }.ToArray();

            var neighborChunks = new[] 
            {
                ChunkLoader.GetNeighborChunk(0, 0, Direction.North),
                ChunkLoader.GetNeighborChunk(0, 0, Direction.East),
                ChunkLoader.GetNeighborChunk(0, 0, Direction.South),
                ChunkLoader.GetNeighborChunk(0, 0, Direction.West),
                ChunkLoader.GetNeighborChunk(0, 0, Direction.NorthEast),
                ChunkLoader.GetNeighborChunk(0, 0, Direction.NorthWest),
                ChunkLoader.GetNeighborChunk(0, 0, Direction.SouthEast),
                ChunkLoader.GetNeighborChunk(0, 0, Direction.SouthWest),
            }.OrderBy(a => a.x).ThenBy(a => a.y).ToArray();

            for (int i=0; i < neighborChunks.Length; i++)
                Assert.That(neighborChunks[i], Is.EqualTo(mapping[i]));
        }

        [Test]
        public void LoadChunksAround_NotIncludedSelf_Correct()
        {
            var loadedChunks = ChunkLoader.GetLoadedChunks();

            // Unload all chunks first
            foreach (var (x, y) in loadedChunks)
                ChunkLoader.UnloadChunk(x, y, true);

            ChunkLoader.LoadChunksAround(0, 0, false);

            loadedChunks = ChunkLoader.GetLoadedChunks();

            var neighbors = ChunkLoader.GetNeighborChunks(0, 0);
            foreach (var (x, y) in neighbors)
                Assert.That(loadedChunks.Any(loadedChunk => loadedChunk.x == x && loadedChunk.y == y));
        }

        [Test]
        public void LoadChunksAround_IncludedSelf_Correct()
        {
            var loadedChunks = ChunkLoader.GetLoadedChunks();

            // Unload all chunks first
            foreach (var (x, y) in loadedChunks)
                ChunkLoader.UnloadChunk(x, y, true);

            ChunkLoader.LoadChunksAround(0, 0, true);

            loadedChunks = ChunkLoader.GetLoadedChunks();

            var neighbors = ChunkLoader.GetNeighborChunks(0, 0).ToList();
            neighbors.Add((0, 0));
            foreach (var (x, y) in neighbors)
                Assert.That(loadedChunks.Any(loadedChunk => loadedChunk.x == x && loadedChunk.y == y));
        }

        [Test]
        public void UnloadNonMandatoryChunks_Correct()
        {
            var loadedChunks = ChunkLoader.GetLoadedChunks();
            Assert.That(loadedChunks, Has.Length.EqualTo(9));

            ChunkLoader.UnloadNonMandatoryChunks();

            var newLoadedChunks = ChunkLoader.GetLoadedChunks();
            Assert.That(newLoadedChunks, Has.Length.EqualTo(loadedChunks.Length));

            for (int i = 0; i < loadedChunks.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(newLoadedChunks[i].x, Is.EqualTo(loadedChunks[i].x));
                    Assert.That(newLoadedChunks[i].y, Is.EqualTo(loadedChunks[i].y));
                });
            }

            // Load useless chunks
            ChunkLoader.LoadChunk(250, 250, out _);
            ChunkLoader.LoadChunk(150, 150, out _);

            newLoadedChunks = ChunkLoader.GetLoadedChunks();
            Assert.That(newLoadedChunks, Has.Length.EqualTo(11));

            ChunkLoader.UnloadNonMandatoryChunks();

            newLoadedChunks = ChunkLoader.GetLoadedChunks();
            Assert.That(newLoadedChunks, Has.Length.EqualTo(9));

            for (int i = 0; i < loadedChunks.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(newLoadedChunks[i].x, Is.EqualTo(loadedChunks[i].x));
                    Assert.That(newLoadedChunks[i].y, Is.EqualTo(loadedChunks[i].y));
                });
            }
        }

        [Test]
        public void GetMandatoryChunks_Get_Correct()
        {
            Grid.Center(0, 0);

            int moduloWidth = (0 % ChunkWidth);
            int moduloHeight = (0 % ChunkHeight);
            var baseChunk = (x: 0 - moduloWidth, y: 0 - moduloHeight);

            // All mandatory chunks
            var mapping = new[]
            {
                (baseChunk.x - ChunkWidth, baseChunk.y - ChunkHeight),
                (baseChunk.x - ChunkWidth, baseChunk.y),
                (baseChunk.x - ChunkWidth, baseChunk.y + ChunkHeight),
                (baseChunk.x, baseChunk.y - ChunkHeight),
                (baseChunk.x, baseChunk.y),
                (baseChunk.x, baseChunk.y + ChunkHeight),
                (baseChunk.x + ChunkWidth, baseChunk.y - ChunkHeight),
                (baseChunk.x + ChunkWidth, baseChunk.y),
                (baseChunk.x + ChunkWidth, baseChunk.y + ChunkHeight)
            }.ToArray();

            var mandatoyChunks = ChunkLoader.GetMandatoryChunks().OrderBy(a => a.x).ThenBy(a => a.y).ToArray();
            Assert.That(mandatoyChunks, Has.Length.EqualTo(mapping.Length));

            foreach (var (x, y) in mandatoyChunks)
                Assert.That(mapping.Any(a => a.Item1 == x && a.Item2 == y));
        }

        [Test]
        public void GetChunkCell_Get_Correct()
        {
            var posX = ChunkLoader.CurrentChunk.x + ChunkWidth / 2;
            var posY = ChunkLoader.CurrentChunk.y + ChunkHeight / 2;

            Cell<int>? cell = null;
            Assert.That(() => cell = ChunkLoader.GetChunkCell(posX, posY), Throws.Nothing);
            Assert.That(cell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(cell.X, Is.EqualTo(posX));
                Assert.That(cell.Y, Is.EqualTo(posY));
            });

            cell = null;
            Assert.Multiple(() =>
            {
                Assert.That(() => cell = ChunkLoader.GetChunkCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5), Throws.Exception);
                Assert.That(() => cell = ChunkLoader.GetChunkCell(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, true), Throws.Nothing);
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
            var cellPositions = new[] { (5, 5), (3, 2), (4, 4) };
            var cells = ChunkLoader.GetChunkCells(cellPositions).ToArray();
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
        public void SetChunkCell_Set_Correct()
        {
            int posX = ChunkLoader.CurrentChunk.x + (ChunkWidth / 2);
            int posY = ChunkLoader.CurrentChunk.y + (ChunkHeight / 2);

            var cell = ChunkLoader.GetChunkCell(posX, posY);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.Walkable, Is.EqualTo(true));

            ChunkLoader.SetChunkCell(new Cell<int>(posX, posY, false, 1), false, null, Grid.IsWorldCoordinateOnScreen, Grid.ScreenCells);

            var changedCell = ChunkLoader.GetChunkCell(posX, posY, false, Grid.IsWorldCoordinateOnScreen, Grid.ScreenCells);
            Assert.That(changedCell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(changedCell.CellType, Is.EqualTo(1));
                Assert.That(() => ChunkLoader.SetChunkCell(new Cell<int>(Grid.Width + 5, Grid.Height + 5, false, 1)), Throws.Nothing);
                Assert.That(() => ChunkLoader.SetChunkCell(new Cell<int>(-Grid.Width - 5, -Grid.Height - 5, false, 1)), Throws.Nothing);
            });
        }

        [Test]
        public void LoadChunk_Load_Correct()
        {
            var loadedChunks = ChunkLoader.GetLoadedChunks();
            var (x, y) = ChunkLoader.GetChunkCoordinate(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5);
            Assert.Multiple(() =>
            {
                Assert.That(!loadedChunks.Any(loadedChunk => loadedChunk.x == x &&
                            loadedChunk.y == y), "(X,Y) chunk was available.");
                Assert.That(loadedChunks.Any(loadedChunk => loadedChunk.x == ChunkLoader.CurrentChunk.x &&
                    loadedChunk.y == ChunkLoader.CurrentChunk.y), "No base chunk available.");

                // Can't load already loaded chunk
                Assert.That(ChunkLoader.LoadChunk(ChunkLoader.CurrentChunk.x, ChunkLoader.CurrentChunk.y, out _), Is.False, "Was able to load base chunk."); 
                Assert.That(ChunkLoader.LoadChunk(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, out _), Is.True, "Was not able to load new chunk.");
            });
            loadedChunks = ChunkLoader.GetLoadedChunks();
            
            Assert.That(loadedChunks.Any(loadedChunk => loadedChunk.x == x && 
                            loadedChunk.y == y));
        }

        [Test]
        public void UnloadChunk_Unload_Correct()
        {
            (int x, int y)[] loadedChunks = ChunkLoader.GetLoadedChunks();
            Assert.That(loadedChunks, Is.Not.Null);
            Assert.That(loadedChunks, Has.Length.EqualTo(9));

            Assert.Multiple(() =>
            {
                Assert.That(loadedChunks.Any(loadedChunk => loadedChunk.x == ChunkLoader.CurrentChunk.x &&
                            loadedChunk.y == ChunkLoader.CurrentChunk.y), "No base chunk loaded");
                // Can't unload mandatory chunk
                Assert.That(ChunkLoader.UnloadChunk(ChunkLoader.CurrentChunk.x, ChunkLoader.CurrentChunk.y), Is.False, "Could unload base chunk");
                // Force unload it
                Assert.That(ChunkLoader.UnloadChunk(ChunkLoader.CurrentChunk.x, ChunkLoader.CurrentChunk.y, true), Is.True, "Could not force unload");
                // Load an arbitrary chunk
                Assert.That(ChunkLoader.LoadChunk(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5, out _), Is.True);
                // See if it can be non force unloaded
                Assert.That(ChunkLoader.UnloadChunk(ViewPortWidth + ChunkWidth * 5, ViewPortHeight + ChunkHeight * 5), Is.True);
            });
            loadedChunks = ChunkLoader.GetLoadedChunks();

            Assert.That(!loadedChunks.Any(loadedChunk => loadedChunk.x == ChunkLoader.CurrentChunk.x &&
                            loadedChunk.y == ChunkLoader.CurrentChunk.y), "Base chunk was still loaded.");

            // Unload all chunks
            foreach (var chunk in loadedChunks)
                ChunkLoader.UnloadChunk(chunk.x, chunk.y, true);
            loadedChunks = ChunkLoader.GetLoadedChunks();
            Assert.Multiple(() =>
            {
                Assert.That(loadedChunks, Has.Length.EqualTo(0));
                Assert.That(ChunkLoader.UnloadChunk(0, 0), Is.False);
            });
        }

        [Test]
        public void GetChunkCoordinate_Get_Correct()
        {
            var (x, y) = ChunkLoader.GetChunkCoordinate(ChunkWidth - 5, ChunkHeight - 5);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(0));
                Assert.That(y, Is.EqualTo(0));
            });
            (x, y) = ChunkLoader.GetChunkCoordinate(ChunkWidth + 5, ChunkHeight + 5);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(ChunkWidth));
                Assert.That(y, Is.EqualTo(ChunkHeight));
            });
            (x, y) = ChunkLoader.GetChunkCoordinate(- 5, -5);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(-ChunkWidth));
                Assert.That(y, Is.EqualTo(-ChunkHeight));
            });
        }

        [Test]
        public void Center_ViewPort_PositiveCoords_Correct()
        {
            var viewPort = Grid.GetViewPortCells().ToArray();
            Assert.That(viewPort.All(cell => cell.CellType != -10), "Initial viewport is not correct");

            var loadedChunks = ChunkLoader.GetLoadedChunks();
            foreach (var (x, y) in loadedChunks)
                ChunkLoader.UnloadChunk(x, y, true);

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
            var cells = Grid.GetCells(positions).ToArray();
            foreach (var cell in cells)
            {
                cell.CellType = -10;
            }
            Grid.SetCells(cells, true);

            int moduloWidth = (100 % ChunkWidth);
            int moduloHeight = (100 % ChunkHeight);
            var baseChunk = (x: 100 - moduloWidth, y: 100 - moduloHeight);

            var cellsUpdated = new List<Cell<int>>();
            Grid.OnCellUpdate += (sender, args) => cellsUpdated.Add(args.Cell);
            loadedChunks = ChunkLoader.GetLoadedChunks();
            Assert.Multiple(() =>
            {
                Assert.That(loadedChunks, Has.Length.EqualTo(0), "Loaded chunks not equal to 0");
                Assert.That(() => Grid.Center(100, 100), Throws.Nothing, "Exception was thrown");
                Assert.That(ChunkLoader.CurrentChunk.x, Is.EqualTo(baseChunk.x), "Current chunk x is not correct");
                Assert.That(ChunkLoader.CurrentChunk.y, Is.EqualTo(baseChunk.y), "Current chunk y is not correct");
            });
            loadedChunks = ChunkLoader.GetLoadedChunks().OrderBy(a => a.x).ThenBy(a => a.y).ToArray();
            Assert.Multiple(() =>
            {
                Assert.That(loadedChunks, Has.Length.EqualTo(9), "Loaded chunks not equal to 9");
                Assert.That(cellsUpdated, Has.Count.EqualTo(viewPort.Length), "cellsUpdated not equal to viewport length");
            });

            var mapping = new[]
            {
                (baseChunk.x - ChunkWidth, baseChunk.y - ChunkHeight),
                (baseChunk.x - ChunkWidth, baseChunk.y),
                (baseChunk.x - ChunkWidth, baseChunk.y + ChunkHeight),
                (baseChunk.x, baseChunk.y - ChunkHeight),
                (baseChunk.x, baseChunk.y),
                (baseChunk.x, baseChunk.y + ChunkHeight),
                (baseChunk.x + ChunkWidth, baseChunk.y - ChunkHeight),
                (baseChunk.x + ChunkWidth, baseChunk.y),
                (baseChunk.x + ChunkWidth, baseChunk.y + ChunkHeight)
            }.ToArray();

            for (int i=0; i < loadedChunks.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(loadedChunks[i].x, Is.EqualTo(mapping[i].Item1), "Mapping x is not correct");
                    Assert.That(loadedChunks[i].y, Is.EqualTo(mapping[i].Item2), "Mapping y is not correct");
                });
            }

            // Check if view port matches now
            Assert.That(Grid.GetViewPortCells().All(cell => cell.CellType == -10), "Viewport cells don't match center changes");
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
                Assert.That(() => Grid.ScreenToWorldCoordinate(-(Grid.Width + 5), -(Grid.Height + 5)), Throws.Exception);
            });
        }

        [Test]
        public void WorldToScreenPoint_Get_Correct()
        {
            var worldPosX = 5;
            var worldPosY = 5;
            var (x, y) = Grid.WorldToScreenCoordinate(worldPosX, worldPosY);
            (int x, int y) _screenPos1 = (0, 0), _screenPos2 = (0, 0), _screenPos3 = (0, 0);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(worldPosX));
                Assert.That(y, Is.EqualTo(worldPosY));
                Assert.That(() => _screenPos1 = Grid.WorldToScreenCoordinate(Grid.Width + 5, Grid.Height + 5), Throws.Nothing);
                Assert.That(() => _screenPos2 = Grid.WorldToScreenCoordinate(-5, -5), Throws.Nothing);
                Assert.That(() => _screenPos3 = Grid.WorldToScreenCoordinate(-(Grid.Width + 5), -(Grid.Height + 5)), Throws.Nothing);
            });
            Assert.Multiple(() =>
            {
                Assert.That(_screenPos1.x, Is.EqualTo(Grid.Width + 5));
                Assert.That(_screenPos1.y, Is.EqualTo(Grid.Height + 5));
                Assert.That(_screenPos2.x, Is.EqualTo(-5));
                Assert.That(_screenPos2.y, Is.EqualTo(-5));
                Assert.That(_screenPos3.x, Is.EqualTo(-(Grid.Width + 5)));
                Assert.That(_screenPos3.y, Is.EqualTo(-(Grid.Height + 5)));
            });
        }

        [Test]
        public void ChunkLoading_CorrectDuring_Centering()
        {
            CenterTowards(0, 0, Direction.North);
            CenterTowards(0, 0, Direction.East);
            CenterTowards(0, 0, Direction.South);
            CenterTowards(0, 0, Direction.West);
        }

        private void CenterTowards(int startX, int startY, Direction dir)
        {
            // Start at 0, 0 => Go in all directions
            Grid.Center(startX, startY);

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
            }

            var loadedChunks = ChunkLoader.GetLoadedChunks();
            Assert.Multiple(() =>
            {
                Assert.That(loadedChunks.Any(a => a.x == 0 && a.y == 0), "Base chunk not loaded!");
                Assert.That(loadedChunks.Any(a => a.x == dirX && a.y == dirY), "New direction chunk not loaded!");
            });

            // Move upwards
            var sizeDir = dirX != 0 ? ChunkWidth : ChunkHeight;
            for (int i = 0; i < sizeDir; i++)
            {
                Grid.Center(dirX != 0 ? (dirX < 0 ? -i : i) : 0, dirY != 0 ? (dirY < 0 ? -i : i) : 0);
                loadedChunks = ChunkLoader.GetLoadedChunks();
                Assert.Multiple(() =>
                {
                    Assert.That(loadedChunks.Any(a => a.x == 0 && a.y == 0), "Chunk (0, 0) not loaded");
                    Assert.That(loadedChunks.Any(a => a.x == dirX && a.y == dirY));
                });
            }

            Grid.Center(dirX != 0 ? dirX : 0, dirY != 0 ? dirY : 0);
            loadedChunks = ChunkLoader.GetLoadedChunks();
            Assert.Multiple(() =>
            {
                Assert.That(loadedChunks.Any(a => a.x == 0 && a.y == 0), "Chunk (0, 0) not loaded");
                Assert.That(!loadedChunks.Any(a => a.x == -dirX && a.y == -dirY));
            });
        }

        [Test]
        public void Center_CellInUnloadedChunk_AvailableOnViewPort_DoesNot_LoadChunk()
        {
            if (ChunkWidth >= (ViewPortWidth / 2) || ChunkHeight >= (ViewPortHeight / 2))
                Assert.Pass("Test ignored because chunk size is higher or equal to half the viewport size.");

            Grid.Center(0, 0);

            var loadedChunks = ChunkLoader.GetLoadedChunks();
            Assert.That(loadedChunks, Has.Length.EqualTo(9));

            int halfViewPortX = ViewPortWidth / 2;
            int halfViewPortY = ViewPortHeight / 2;

            // Get cell out of chunk range, but within viewport
            var (x, y) = (halfViewPortX - (ChunkWidth / 2), halfViewPortY - (ChunkHeight / 2));
            var (x2, y2) = (-halfViewPortX + (ChunkWidth / 2), -halfViewPortY + (ChunkHeight / 2));

            // Check both coordinates are on screen
            Assert.Multiple(() =>
            {
                Assert.That(Grid.IsWorldCoordinateOnScreen(x, y, out _, out _), Is.True);
                Assert.That(Grid.IsWorldCoordinateOnScreen(x2, y2, out _, out _), Is.True);
            });

            // Chunks retrieved are null without proper method call
            Assert.That(() => ChunkLoader.GetChunkCell(x, y, false), Throws.Exception);
            Assert.That(() => ChunkLoader.GetChunkCell(x2, y2, false), Throws.Exception);

            // Chunks retrieved are not null with proper method call with load chunks false
            var cells = new[]
            {
                ChunkLoader.GetChunkCell(x, y, false, Grid.IsWorldCoordinateOnScreen, Grid.ScreenCells),
                ChunkLoader.GetChunkCell(x2, y2, false, Grid.IsWorldCoordinateOnScreen, Grid.ScreenCells)
            };
            Assert.That(cells.All(a => a != null));
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
        public void GetCell_ReturnsCorrectChunkScreenCell_IfScreenCell_WasNotYetAdjusted()
        {
            AdjustProceduralGridGen(Seed, (rand, chunk, width, height) =>
            {
                // Set default to -5 on all cells
                for (int x=0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        chunk[y * width + x] = -5;
                    }
                }
            });

            // Verify that all cells have this default value set properly
            var viewPortCells = Grid.GetViewPortCells().ToArray();
            Assert.That(viewPortCells.Count(a => a.CellType == -5), Is.EqualTo(viewPortCells.Length));

            // Verify that GetCell has this default value set properly
            var cell = Grid.GetCell(0, 0);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(-5));

            // Verify that GetCells has this default value set properly
            var cells = Grid.GetCells(new[] { (0, 0) });
            cell = cells.SingleOrDefault();
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(-5));
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
        public void GenerateChunk_ChunkData_IsAlwaysSame()
        {
            int chunkX = ViewPortWidth + ChunkWidth * 10;
            int chunkY = ViewPortHeight + ChunkHeight * 10;
            var chunkCoords = ChunkLoader.GetChunkCoordinate(chunkX, chunkY);

            Assert.That(() => ChunkLoader.LoadChunk(chunkX, chunkY, out _), Is.True);

            // Collect current chunk data
            int[] chunkData = new int[ChunkWidth * ChunkHeight];
            for (int x=0; x < ChunkWidth; x++)
            {
                for (int y = 0; y < ChunkHeight; y++)
                {
                    var cell = Grid.GetCell(chunkCoords.x + x, chunkCoords.y + y);
                    Assert.That(cell, Is.Not.Null);
                    chunkData[y * ChunkWidth + x] = cell.CellType;
                }
            }

            Assert.That(() => ChunkLoader.UnloadChunk(chunkX, chunkY), Is.True);

            bool eventRaised = false;
            void genCheck(object? sender, int[] chunk)
            {
                Assert.That(chunkData.SequenceEqual(chunk));
                eventRaised = true;
            }
            onGenerateChunk += genCheck;
            Assert.Multiple(() =>
            {
                Assert.That(() => ChunkLoader.LoadChunk(chunkX, chunkY, out _), Is.True);
                Assert.That(eventRaised, Is.True);
            });
            onGenerateChunk -= genCheck;
        }

        [Test]
        public void ClearGridCache_Throws_NoException()
        {
            // Populate the grid cache
            var cells = new List<Cell<int>>();
            for (int x=Grid.Width / 2; x < (Grid.Width / 2) + 10; x++)
            {
                for (int y = Grid.Height / 2; y < (Grid.Height / 2) + 10; y++)
                {
                    cells.Add(new Cell<int>(x, y, -10));
                }
            }

            List<Cell<int>> prevState = Grid.GetCells(cells.Select(a => (a.X, a.Y))).ToList();
            Grid.SetCells(cells, true);

            Assert.That(() => Grid.ClearCache(), Throws.Nothing);

            cells = Grid.GetCells(cells.Select(a => (a.X, a.Y))).ToList();

            Assert.That(cells.SequenceEqual(prevState, new CellFullComparer<int>()), "Cells are not reset.");
        }
    }
}
