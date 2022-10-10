using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Venomaus.FlowVitae.Cells;

namespace Venomaus.BenchmarkTests.Benchmarks.Cases.ProceduralGridCases
{
    public class ProcGenBaseBenchmarks : BaseGridBenchmarks<int, Cell<int>>
    {
        protected override int Seed => 1000;
        protected override bool ProcGenEnabled => true;

        private (int x, int y) NextChunkCoordinate;
        private (int x, int y) SameChunkPlus1Pos;

        protected override void GenerateChunk(Random random, int[] chunk, int width, int height)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    chunk[y * width + x] = random.Next(-10, 10);
        }

        private (int x, int y) GetChunkCoordinate(int x, int y)
        {
            if (x < 0 && x % ChunkWidth != 0) x -= ChunkWidth;
            if (y < 0 && y % ChunkHeight != 0) y -= ChunkHeight;
            var chunkX = ChunkWidth * (x / ChunkWidth);
            var chunkY = ChunkHeight * (y / ChunkHeight);
            return (chunkX, chunkY);
        }

        public override void Setup()
        {
            base.Setup();

            Program.WriteLine("Setup override is called!");

            var (x, y) = (Grid.Width / 2, Grid.Height / 2);
            NextChunkCoordinate = GetChunkCoordinate(x + ChunkWidth, y);

            var center = (x: Grid.Width / 2, y: Grid.Height / 2);
            SameChunkPlus1Pos = (center.x + 1, center.y);
        }

        [Benchmark]
        public void SetCell_NoStoreState()
        {
            Grid.SetCell(ViewPortWidth / 2, ViewPortHeight / 2, 5);
        }

        [Benchmark]
        public void SetCell_WithStoreState()
        {
            Grid.SetCell(ViewPortWidth / 2, ViewPortHeight / 2, 5, true);
        }

        [Benchmark]
        public void SetCells_NoStoreState()
        {
            Grid.SetCells(ProceduralCells);
        }

        [Benchmark]
        public void SetCells_WithStoreState()
        {
            Grid.SetCells(ProceduralCells, true);
        }

        [Benchmark]
        public int GetCellType_NoChunkLoad()
        {
            return Grid.GetCellType(ViewPortWidth / 2, ViewPortHeight / 2);
        }

        [Benchmark]
        public int GetCellType_WithChunkLoad()
        {
            return Grid.GetCellType(ViewPortWidth + ChunkWidth * 5, ViewPortWidth + ChunkHeight * 5);
        }

        [Benchmark]
        public Cell<int>? GetCell_NoChunkLoad()
        {
            return Grid.GetCell(ViewPortWidth / 2, ViewPortHeight / 2);
        }

        [Benchmark]
        public Cell<int>? GetCell_WithChunkLoad()
        {
            return Grid.GetCell(ViewPortWidth + ChunkWidth * 5, ViewPortWidth + ChunkHeight * 5);
        }

        [Benchmark]
        public void GetCells_NoChunkLoad()
        {
            Grid.GetCells(ProceduralPositionsInView).Consume(Consumer);
        }

        [Benchmark]
        public void GetCells_WithChunkLoad()
        {
            Grid.GetCells(ProceduralPositions).Consume(Consumer);
        }

        [Benchmark]
        public void Center_NextChunk()
        {
            Grid.Center(NextChunkCoordinate.x, NextChunkCoordinate.y);
        }

        [Benchmark]
        public void Center_SameChunk()
        {
            Grid.Center(SameChunkPlus1Pos.x, SameChunkPlus1Pos.y);
        }

        [Benchmark]
        public void GetViewPortCells()
        {
            Grid.GetViewPortCells().Consume(Consumer);
        }
    }
}
