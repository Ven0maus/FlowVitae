using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Venomaus.FlowVitae.Basics.Chunking;
using Venomaus.FlowVitae.Cells;

namespace Venomaus.BenchmarkTests.Benchmarks.Cases.StaticGridCases
{
    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.SlowestToFastest)]
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
        public int GetCellType()
        {
            return Grid.GetCellType(ViewPortWidth / 2, ViewPortHeight / 2);
        }

        [Benchmark]
        public Cell<int>? GetCell()
        {
            return Grid.GetCell(ViewPortWidth / 2, ViewPortHeight / 2);
        }

        [Benchmark]
        public void GetCells()
        {
            Grid.GetCells(Positions).Consume(Consumer);
        }

        [Benchmark]
        public void GetViewPortWorldCoordinates()
        {
            Grid.GetViewPortWorldCoordinates().Consume(Consumer);
        }
    }
}
