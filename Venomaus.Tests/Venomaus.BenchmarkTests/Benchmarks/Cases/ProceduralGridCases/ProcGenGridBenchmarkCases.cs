using BenchmarkDotNet.Attributes;
using Venomaus.FlowVitae.Cells;

namespace Venomaus.BenchmarkTests.Benchmarks.Cases.ProceduralGridCases
{
    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.SlowestToFastest)]
    public class ProcGenGridBenchmarkCases : BaseGridBenchmarks<int, Cell<int>>
    {
        protected override int Seed => 1000;
        protected override bool ProcGenEnabled => true;

        protected override void GenerateChunk(Random random, int[] chunk, int width, int height)
        {
            for (int x=0; x < width; x++)
                for (int y = 0; y < height; x++)
                    chunk[y * width + x] = random.Next(-10, 10);
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
        public void Center()
        {
            var posX = Random.Next(-(ViewPortWidth + ChunkWidth * 3), ViewPortWidth + ChunkWidth * 3);
            var posY = Random.Next(-(ViewPortWidth + ChunkWidth * 3), ViewPortWidth + ChunkWidth * 3);
            Grid.Center(posX, posY);
        }

        [Benchmark]
        public void GetViewPortCells()
        {
            Grid.GetViewPortCells();
        }
    }
}
