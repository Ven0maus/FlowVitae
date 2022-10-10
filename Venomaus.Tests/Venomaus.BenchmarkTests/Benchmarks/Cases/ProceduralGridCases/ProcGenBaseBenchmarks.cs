using BenchmarkDotNet.Attributes;
using Venomaus.FlowVitae.Cells;

namespace Venomaus.BenchmarkTests.Benchmarks.Cases.ProceduralGridCases
{
    public class ProcGenBaseBenchmarks : BaseGridBenchmarks<int, Cell<int>>
    {
        protected override int Seed => 1000;
        protected override bool ProcGenEnabled => true;

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
        public void GetCellType_NoChunkLoad()
        {
            Grid.GetCellType(ViewPortWidth / 2, ViewPortHeight / 2);
        }

        [Benchmark]
        public void GetCellType_WithChunkLoad()
        {
            Grid.GetCellType(ViewPortWidth + ChunkWidth * 5, ViewPortWidth + ChunkHeight * 5);
        }

        [Benchmark]
        public void GetCell_NoChunkLoad()
        {
            Grid.GetCell(ViewPortWidth / 2, ViewPortHeight / 2);
        }

        [Benchmark]
        public void GetCell_WithChunkLoad()
        {
            Grid.GetCell(ViewPortWidth + ChunkWidth * 5, ViewPortWidth + ChunkHeight * 5);
        }

        [Benchmark]
        public void GetCells_NoChunkLoad()
        {
            Grid.GetCells(ProceduralPositionsInView);
        }

        [Benchmark]
        public void GetCells_WithChunkLoad()
        {
            Grid.GetCells(ProceduralPositions);
        }

        [Benchmark]
        public void Center_NextChunk()
        {
            var center = (x: Grid.Width / 2, y: Grid.Height / 2);
            var chunkCoord = GetChunkCoordinate(center.x, center.y);
            // Move center to the next chunk
            Grid.Center(chunkCoord.x + ChunkWidth, center.y);
        }

        [Benchmark]
        public void Center_SameChunk()
        {
            var center = (x: Grid.Width / 2, y: Grid.Height / 2);
            Grid.Center(center.x + 1, center.y);
        }

        [Benchmark]
        public void GetViewPortCells()
        {
            Grid.GetViewPortCells();
        }
    }
}
