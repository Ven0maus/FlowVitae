using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Chunking.Generators;

namespace Venomaus.UnitTests.Tests
{
    internal class StaticChunkLoaderTests : ChunkLoaderTests
    {
        private const int NullCell = -1000;
        private IProceduralGen<int, Cell<int>> _procGen;
        protected override IProceduralGen<int, Cell<int>> ProcGen => _procGen;

        private readonly int[] _baseMap;

        private int[] GenerateBaseMap(int width, int height)
        {
            var chunk = new int[width * height];
            var random = new Random(0);
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
            int chunkX = ViewPortWidth + ChunkWidth * 10;
            int chunkY = ViewPortHeight + ChunkHeight * 10;
            var chunkCoords = ChunkLoader.GetChunkCoordinate(chunkX, chunkY);

            Assert.That(() => ChunkLoader.LoadChunk(chunkX, chunkY, out _), Is.True);

            // Collect current chunk data
            int[] chunkData = new int[ChunkWidth * ChunkHeight];
            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int y = 0; y < ChunkHeight; y++)
                {
                    var cell = Grid.GetCell(chunkCoords.x + x, chunkCoords.y + y);
                    Assert.That(cell, Is.Not.Null);
                    chunkData[y * ChunkWidth + x] = cell.CellType;
                }
            }

            Assert.That(() => ChunkLoader.UnloadChunk(chunkX, chunkY), Is.True);
            Assert.That(() => ChunkLoader.LoadChunk(chunkX, chunkY, out _), Is.True);

            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int y = 0; y < ChunkHeight; y++)
                {
                    var cell = Grid.GetCell(chunkCoords.x + x, chunkCoords.y + y);
                    Assert.Multiple(() =>
                    {
                        Assert.That(cell, Is.Not.Null);
                        Assert.That(chunkData[y * ChunkWidth + x], Is.EqualTo(cell.CellType));
                    });
                }
            }
        }
    }
}
