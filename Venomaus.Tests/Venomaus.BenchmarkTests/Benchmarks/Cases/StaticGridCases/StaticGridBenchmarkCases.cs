using BenchmarkDotNet.Attributes;
using Venomaus.FlowVitae.Cells;

namespace Venomaus.BenchmarkTests.Benchmarks.Cases.StaticGridCases
{
    [MemoryDiagnoser]
    public class StaticGridBenchmarkCases : BaseGridBenchmarks<int, Cell<int>>
    {
        protected override int Seed => 1000;
        protected override bool ProcGenEnabled => false;

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
            Grid.SetCells(Cells);
        }

        [Benchmark]
        public void SetCells_WithStoreState()
        {
            Grid.SetCells(Cells, true);
        }

        [Benchmark]
        public void GetCellType()
        {
            Grid.GetCellType(ViewPortWidth / 2, ViewPortHeight / 2);
        }

        [Benchmark]
        public void GetCell()
        {
            Grid.GetCell(ViewPortWidth / 2, ViewPortHeight / 2);
        }

        [Benchmark]
        public void GetCells()
        {
            Grid.GetCells(Positions);
        }

        [Benchmark]
        public void Center()
        {
            var posX = Random.Next(0, ViewPortWidth);
            var posY = Random.Next(0, ViewPortHeight);
            Grid.Center(posX, posY);
        }

        [Benchmark]
        public void GetViewPortCells()
        {
            Grid.GetViewPortCells();
        }
    }
}
