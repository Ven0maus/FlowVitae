using Venomaus.FlowVitae.Basics.Chunking;
using Venomaus.FlowVitae.Basics.Procedural;
using Venomaus.Tests.TestObjects;
using Direction = Venomaus.FlowVitae.Helpers.Direction;

namespace Venomaus.Tests.ImplTests
{
    internal class ChunkLoaderTests : BaseTests<int, TestCell<int>>
    {
        private const int Seed = 1000;
        protected override IProceduralGen<int, TestCell<int>>? ProcGen => new ProceduralGenerator<int, TestCell<int>>(Seed, GenerateChunk);

        private void GenerateChunk(Random random, int[] chunk, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    chunk[y * width + x] = random.Next(0, 10);
                }
            }
        }

        private ChunkLoader<int, TestCell<int>> ChunkLoader { get { return Grid._chunkLoader ?? throw new Exception("Chunkloader not initialized"); } }

        [Test]
        public void ChunkLoader_Is_Not_Null()
        {
            Assert.That(() => ChunkLoader, Throws.Nothing);
        }

        [Test]
        public void StoreState_SetAndGet_Correct()
        {
            // Check if original cell is not 4
            var cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.Not.EqualTo(4));

            // Change cell to 4 with store state
            Grid.SetCell(new TestCell<int>(5, 5, 4, 10), true);

            // Verify if cell is 4 and number matches stored state
            cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(4));
            Assert.That(cell.Number, Is.EqualTo(10));

            // Set cell to 1 with no store state
            Grid.SetCell(5, 5, 1, false);

            // Verify if cell is 1 and number is default again
            cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(1));
            Assert.That(cell.Number, Is.EqualTo(default(int)));
        }

        [Test]
        public void PositiveCoordinate_SetAndGet_Correct()
        {
            // When not saving state in an unloaded chunk, the chunk data is lost
            Grid.SetCell(55, 72, -5);
            var cell = Grid.GetCell(55, 72);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.Not.EqualTo(-5));

            // When saving state in an unloaded chunk, the chunk data is stored
            Grid.SetCell(55, 72, -5, true);
            cell = Grid.GetCell(55, 72);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(-5));
        }

        [Test]
        public void NegativeCoordinate_SetAndGet_Correct()
        {
            // When not saving state in an unloaded chunk, the chunk data is lost
            Grid.SetCell(-55, -72, -5);
            var cell = Grid.GetCell(-55, -72);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.Not.EqualTo(-5));

            // When saving state in an unloaded chunk, the chunk data is stored
            Grid.SetCell(-55, -72, -5, true);
            cell = Grid.GetCell(-55, -72);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(-5));
        }

        [Test]
        public void CurrentChunk_GetAndSet_Correct()
        {
            var current = ChunkLoader.CurrentChunk;
            var chunkCoordinate = ChunkLoader.GetChunkCoordinate(Grid.Width / 2, Grid.Height /2);
            Assert.That(current.x, Is.EqualTo(chunkCoordinate.x));
            Assert.That(current.y, Is.EqualTo(chunkCoordinate.y));

            Grid.Center(30, 30);

            current = ChunkLoader.CurrentChunk;
            Assert.That(current.x, Is.Not.EqualTo(chunkCoordinate.x));
            Assert.That(current.y, Is.Not.EqualTo(chunkCoordinate.y));

            chunkCoordinate = ChunkLoader.GetChunkCoordinate(30, 30);
            Assert.That(current.x, Is.EqualTo(chunkCoordinate.x));
            Assert.That(current.y, Is.EqualTo(chunkCoordinate.y));
        }

        [Test]
        public void WorldCoordinateToChunkCoordinate_Remapping_Correct()
        {
            // Current chunk
            var remappedCoordinateOnChunk = ChunkLoader.RemapChunkCoordinate(5, 5);
            Assert.That(remappedCoordinateOnChunk.x, Is.EqualTo(5));
            Assert.That(remappedCoordinateOnChunk.y, Is.EqualTo(5));

            // Positive coord in another chunk
            remappedCoordinateOnChunk = ChunkLoader.RemapChunkCoordinate(25, 25);
            Assert.That(remappedCoordinateOnChunk.x, Is.EqualTo(0));
            Assert.That(remappedCoordinateOnChunk.y, Is.EqualTo(0));

            // Negative coord
            remappedCoordinateOnChunk = ChunkLoader.RemapChunkCoordinate(-5, -5);
            Assert.That(remappedCoordinateOnChunk.x, Is.EqualTo(20));
            Assert.That(remappedCoordinateOnChunk.y, Is.EqualTo(20));
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
            };

            var mapping = new[]
            {
                (0, 25),
                (25, 0),
                (0, -25),
                (-25, 0),
                (25, 25),
                (-25, 25),
                (25, -25),
                (-25, -25)
            };

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
            ChunkLoader.LoadChunk(250, 250);
            ChunkLoader.LoadChunk(150, 150);

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
            // All mandatory chunks
            var mapping = new[]
            {
                (0, 0),
                (0, 25),
                (25, 0),
                (0, -25),
                (-25, 0),
                (25, 25),
                (-25, 25),
                (25, -25),
                (-25, -25)
            };

            var mandatoyChunks = ChunkLoader.GetMandatoryChunks();
            Assert.That(mandatoyChunks, Has.Count.EqualTo(mapping.Length));

            foreach (var (x, y) in mandatoyChunks)
                Assert.That(mapping.Any(a => a.Item1 == x && a.Item2 == y));
        }

        [Test]
        public void GetChunkCell_Get_Correct()
        {
            TestCell<int>? cell = null;
            Assert.That(() => cell = ChunkLoader.GetChunkCell(5, 5), Throws.Nothing);
            Assert.That(cell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(cell.X, Is.EqualTo(5));
                Assert.That(cell.Y, Is.EqualTo(5));
            });

            Assert.That(() => cell = ChunkLoader.GetChunkCell(150, 150), Throws.Nothing);
            Assert.That(cell, Is.Null);

            Assert.That(() => cell = ChunkLoader.GetChunkCell(150, 150, true), Throws.Nothing);
            Assert.That(cell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(cell.X, Is.EqualTo(150));
                Assert.That(cell.Y, Is.EqualTo(150));
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
            var cell = ChunkLoader.GetChunkCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(5));

            ChunkLoader.SetChunkCell(new TestCell<int>(5, 5, 1, 0));

            var changedCell = ChunkLoader.GetChunkCell(5, 5);
            Assert.That(changedCell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(changedCell.CellType, Is.EqualTo(1));
                Assert.That(() => ChunkLoader.SetChunkCell(new TestCell<int>(Grid.Width + 5, Grid.Height + 5, 1, 0)), Throws.Nothing);
                Assert.That(() => ChunkLoader.SetChunkCell(new TestCell<int>(-5, -5, 1, 0)), Throws.Nothing);
            });
        }

        [Test]
        public void LoadChunk_Load_Correct()
        {
            var loadedChunks = ChunkLoader.GetLoadedChunks();
            var (x, y) = ChunkLoader.GetChunkCoordinate(57, 49);
            Assert.Multiple(() =>
            {
                Assert.That(!loadedChunks.Any(loadedChunk => loadedChunk.x == x &&
                            loadedChunk.y == y));
                Assert.That(loadedChunks.Any(loadedChunk => loadedChunk.x == 0 &&
                    loadedChunk.y == 0));

                // Can't load already loaded chunk
                Assert.That(ChunkLoader.LoadChunk(0, 0), Is.False); 
                Assert.That(ChunkLoader.LoadChunk(57, 49), Is.True);
            });
            loadedChunks = ChunkLoader.GetLoadedChunks();
            
            Assert.That(loadedChunks.Any(loadedChunk => loadedChunk.x == x && 
                            loadedChunk.y == y));
        }

        [Test]
        public void UnloadChunk_Unload_Correct()
        {
            var loadedChunks = ChunkLoader.GetLoadedChunks();
            Assert.Multiple(() =>
            {
                Assert.That(loadedChunks.Any(loadedChunk => loadedChunk.x == 0 &&
                            loadedChunk.y == 0));
                // Can't unload mandatory chunk
                Assert.That(ChunkLoader.UnloadChunk(0, 0), Is.False);
                // Force unload it
                Assert.That(ChunkLoader.UnloadChunk(0, 0, true), Is.True);
                // Load an arbitrary chunk
                Assert.That(ChunkLoader.LoadChunk(57, 49), Is.True);
                // See if it can be non force unloaded
                Assert.That(ChunkLoader.UnloadChunk(57, 49), Is.True);
            });
            loadedChunks = ChunkLoader.GetLoadedChunks();

            Assert.That(!loadedChunks.Any(loadedChunk => loadedChunk.x == 0 &&
                            loadedChunk.y == 0));
        }

        [Test]
        public void GetChunkCoordinate_Get_Correct()
        {
            var (x, y) = ChunkLoader.GetChunkCoordinate(5, 5);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(0));
                Assert.That(y, Is.EqualTo(0));
            });
            (x, y) = ChunkLoader.GetChunkCoordinate(27, 27);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(25));
                Assert.That(y, Is.EqualTo(25));
            });
            (x, y) = ChunkLoader.GetChunkCoordinate(-5, -5);
            Assert.Multiple(() =>
            {
                Assert.That(x, Is.EqualTo(-25));
                Assert.That(y, Is.EqualTo(-25));
            });
        }

        [Test]
        public void Center_ViewPort_PositiveCoords_Correct()
        {
            var viewPort = Grid.GetViewPortCells();
            Assert.That(viewPort.All(cell => cell.CellType != -10));

            var loadedChunks = ChunkLoader.GetLoadedChunks();
            foreach (var (x, y) in loadedChunks)
                ChunkLoader.UnloadChunk(x, y, true);

            // Set cells beforehand
            var positions = new List<(int x, int y)>();
            for (int x = 88; x < 113; x++)
            {
                for (int y = 88; y < 113; y++)
                {
                    positions.Add((x, y));
                }
            }
            var cells = Grid.GetCells(positions);
            foreach (var cell in cells)
            {
                cell.CellType = -10;
            }
            Grid.SetCells(cells, true);

            loadedChunks = ChunkLoader.GetLoadedChunks();
            Assert.Multiple(() =>
            {
                Assert.That(loadedChunks, Has.Length.EqualTo(0));
                Assert.That(() => Grid.Center(100, 100), Throws.Nothing);
                Assert.That(ChunkLoader.CurrentChunk.x, Is.EqualTo(100));
                Assert.That(ChunkLoader.CurrentChunk.y, Is.EqualTo(100));
            });
            loadedChunks = ChunkLoader.GetLoadedChunks().OrderBy(a => a.x).ThenBy(a => a.y).ToArray();
            Assert.That(loadedChunks, Has.Length.EqualTo(9));

            var mapping = new[]
            {
                (100, 100),
                (100, 125),
                (125, 100),
                (100, 75),
                (75, 100),
                (125, 125),
                (75, 125),
                (125, 75),
                (75, 75)
            }.OrderBy(a => a.Item1).ThenBy(a => a.Item2).ToArray();

            for (int i=0; i < loadedChunks.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(loadedChunks[i].x, Is.EqualTo(mapping[i].Item1));
                    Assert.That(loadedChunks[i].y, Is.EqualTo(mapping[i].Item2));
                });
            }

            // Check if view port matches now
            viewPort = Grid.GetViewPortCells();
            Assert.That(viewPort.All(cell => cell.CellType == -10));
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
                Assert.That(_screenPos1.x, Is.EqualTo(30));
                Assert.That(_screenPos1.y, Is.EqualTo(30));
                Assert.That(_screenPos2.x, Is.EqualTo(-5));
                Assert.That(_screenPos2.y, Is.EqualTo(-5));
                Assert.That(_screenPos3.x, Is.EqualTo(-30));
                Assert.That(_screenPos3.y, Is.EqualTo(-30));
            });
        }
    }
}
