using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Chunking.Generators;
using Venomaus.FlowVitae.Grids;
using Venomaus.FlowVitae.Helpers;
using Venomaus.UnitTests.Tools;

namespace Venomaus.UnitTests.Tests
{
    internal class StaticChunkLoaderTests : ChunkLoaderTests
    {
        private const int NullCell = -1000;
        private const int Seed = 0;
        private readonly IProceduralGen<int, Cell<int>> _procGen;
        protected override IProceduralGen<int, Cell<int>> ProcGen => _procGen;

        private readonly int[] _baseMap;

        private int[] GenerateBaseMap(int width, int height)
        {
            var chunk = new int[width * height];
            var random = new Random(Seed);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    chunk[y * width + x] = random.Next(0, 10);
                }
            }
            InvokeChunkGenerationEvent(this, chunk);
            return chunk;
        }

        public StaticChunkLoaderTests(int viewPortWidth, int viewPortHeight, int chunkWidth, int chunkHeight) 
            : base(viewPortWidth, viewPortHeight, chunkWidth, chunkHeight)
        {
            _baseMap = GenerateBaseMap(viewPortWidth, viewPortHeight);
            _procGen = new StaticGenerator<int, Cell<int>>(_baseMap, viewPortWidth, viewPortHeight, NullCell);
        }

        [Test]
        public override void GenerateChunk_ChunkData_IsAlwaysSame()
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

            Assert.That(() => chunkLoader.UnloadChunk(chunkX, chunkY), Is.True);
            Assert.That(() => chunkLoader.LoadChunk(chunkX, chunkY, out _), Is.True);

            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int y = 0; y < ChunkHeight; y++)
                {
                    var cell = grid.GetCell(chunkCoords.x + x, chunkCoords.y + y);
                    Assert.Multiple(() =>
                    {
                        Assert.That(cell, Is.Not.Null);
                        if (cell != null)
                            Assert.That(chunkData[y * ChunkWidth + x], Is.EqualTo(cell.CellType));
                    });
                }
            }
        }

        [Test]
        public override void GetChunkData_Returns_ValidData()
        {
            // Custom chunk generation implementation
            Func<int, int[], int, int, (int x, int y), TestChunkData> chunkGenerationMethod = (seed, baseMap, width, height, chunkCoordinate) =>
            {
                // Define custom chunk data
                var chunkData = new TestChunkData
                {
                    Trees = new HashSet<(int x, int y)>(new TupleComparer<int>())
                };
                var random = new Random(ProcGen.Seed);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        // Every chunk will have a tree at 0, 0
                        if (x == 0 && y == 0)
                            chunkData.Trees.Add((x, y));
                        baseMap[y * width + x] = random.Next(-10, 10);
                    }
                }
                return chunkData;
            };

            // Initialize the custom implementations
            var customProcGen = new StaticGenerator<int, Cell<int>, TestChunkData>(_baseMap, ViewPortWidth, ViewPortHeight, NullCell, chunkGenerationMethod);
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
            var seed = Fnv1a.Hash32(chunkCoordinate.x, chunkCoordinate.y, ProcGen.Seed);
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
    }
}
