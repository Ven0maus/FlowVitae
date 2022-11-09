using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Grids;
using Venomaus.FlowVitae.Chunking.Generators;

namespace Venomaus.BenchmarkTests.Benchmarks
{
    [HideColumns("ViewPortWidth", "ViewPortHeight", "ChunkWidth", "ChunkHeight")]
    public abstract class BaseGridBenchmarks<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        protected Grid<TCellType, TCell> Grid { get; private set; }

        [ParamsSource(nameof(ValueViewPortWidth))]
        public int ViewPortWidth { get; set; }

        [ParamsSource(nameof(ValueViewPortHeight))]
        public int ViewPortHeight { get; set; }

        [ParamsSource(nameof(ValueChunkWidth))]
        public int ChunkWidth { get; set; }

        [ParamsSource(nameof(ValueChunkHeight))]
        public int ChunkHeight { get; set; }

        protected readonly Consumer Consumer = new();

        protected virtual bool ProcGenEnabled { get; }
        protected virtual bool DivideChunk { get; }

        protected Cell<int>[] Cells { get; private set; }
        protected Cell<int>[] ProceduralCells { get; private set; }
        protected (int x, int y)[] Positions { get; private set; }
        protected (int x, int y)[] ProceduralPositions { get; private set; }
        protected (int x, int y)[] ProceduralPositionsInView { get; private set; }
        protected abstract int Seed { get; }
        protected Random Random { get; private set; }

        protected (int x, int y) NextChunkCoordinate;
        protected (int x, int y) SameChunkPlus1Pos;

        [GlobalSetup]
        public void Setup()
        {
            Grid = new Grid<TCellType, TCell>(ViewPortWidth, ViewPortHeight, DivideChunk ? (int)((double)ViewPortWidth / 100 * 30) : ChunkWidth, DivideChunk ? (int)((double)ViewPortHeight / 100 * 30) : ChunkHeight, InitializeProcGen());

            // Initialize benchmark data
            Random = new Random(Seed);
            Cells = PopulateCellsArray(false);
            ProceduralCells = PopulateCellsArray(true);
            Positions = PopulatePositionsArray(false, false);
            ProceduralPositions = PopulatePositionsArray(true, false);
            ProceduralPositionsInView = PopulatePositionsArray(true, true);

            var (x, y) = (Grid.Width / 2, Grid.Height / 2);
            NextChunkCoordinate = GetChunkCoordinate(x + ChunkWidth, y);

            var center = (x: Grid.Width / 2, y: Grid.Height / 2);
            SameChunkPlus1Pos = (center.x + 1, center.y);
        }

        private (int x, int y) GetChunkCoordinate(int x, int y)
        {
            if (x < 0 && x % ChunkWidth != 0) x -= ChunkWidth;
            if (y < 0 && y % ChunkHeight != 0) y -= ChunkHeight;
            var chunkX = ChunkWidth * (x / ChunkWidth);
            var chunkY = ChunkHeight * (y / ChunkHeight);
            return (chunkX, chunkY);
        }

        public static IEnumerable<int> ValueViewPortWidth()
        {
            yield return Program.GetBenchmarkSettings().ViewPortWidth;
        }

        public static IEnumerable<int> ValueViewPortHeight()
        {
            yield return Program.GetBenchmarkSettings().ViewPortHeight;
        }

        public static IEnumerable<int> ValueChunkWidth()
        {
            yield return Program.GetBenchmarkSettings().ChunkWidth;
        }

        public static IEnumerable<int> ValueChunkHeight()
        {
            yield return Program.GetBenchmarkSettings().ChunkHeight;
        }

        protected IProceduralGen<TCellType, TCell>? InitializeProcGen()
        {
            return ProcGenEnabled ? new ProceduralGenerator<TCellType, TCell>(Seed, GenerateChunk) : null;
        }

        protected virtual void GenerateChunk(Random random, TCellType[] chunk, int width, int height, (int x, int y) chunkCoordinate)
        { }

        protected virtual (int x, int y)[] PopulatePositionsArray(bool procedural, bool inView)
        {
            var width10Procent = (int)((double)ChunkWidth / 100 * 10);
            var height10Procent = (int)((double)ChunkHeight / 100 * 10);
            var positions = new (int x, int y)[width10Procent * height10Procent];

            var center = (x: ViewPortWidth / 2, y: ViewPortHeight / 2);
            var minCoord = (x: center.x - center.x, y: center.y - center.y);
            var maxCoord = (x: center.x + center.x, y: center.y + center.y);

            for (int x = 0; x < width10Procent; x++)
            {
                for (int y = 0; y < height10Procent; y++)
                {
                    if (procedural) 
                    {
                        int randX, randY;
                        if (inView)
                        {
                            randX = Random.Next(minCoord.x, maxCoord.x);
                            randY = Random.Next(minCoord.y, maxCoord.y);
                        }
                        else
                        {
                            randX = Random.Next(-(ViewPortWidth * ChunkWidth * 5), ViewPortWidth * ChunkWidth * 5);
                            randY = Random.Next(-(ViewPortHeight * ChunkHeight * 5), ViewPortHeight * ChunkHeight * 5);
                        }
                        positions[y * width10Procent + x] = (randX, randY);
                    }
                    else 
                    {
                        positions[y * width10Procent + x] = (x, y);
                    }
                }
            }

            return positions;
        }

        protected virtual Cell<int>[] PopulateCellsArray(bool procedural)
        {
            var width10Procent = (int)((double)ChunkWidth / 100 * 10);
            var height10Procent = (int)((double)ChunkHeight / 100 * 10);
            var cells = new Cell<int>[width10Procent * height10Procent];

            for (int x = 0; x < width10Procent; x++)
            {
                for (int y = 0; y < height10Procent; y++)
                {
                    if (procedural)
                    {
                        var randX = Random.Next(-(ViewPortWidth * ChunkWidth * 5), ViewPortWidth * ChunkWidth * 5);
                        var randY = Random.Next(-(ViewPortHeight * ChunkHeight * 5), ViewPortHeight * ChunkHeight * 5);
                        cells[y * width10Procent + x] = new Cell<int>(randX, randY, Random.Next(-10, 11));
                    }
                    else
                    {
                        cells[y * width10Procent + x] = new Cell<int>(x, y, Random.Next(-10, 11));
                    }
                }
            }

            return cells;
        }
    }
}
