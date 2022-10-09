using Venomaus.BenchmarkTests.Benchmarks.Cases.ProceduralGridCases;

namespace Venomaus.BenchmarkTests.Benchmarks.Configurations.ProceduralGrids.DifferentChunkSize
{
    internal class SmallDSProcGenBenchmarks : ProcGenGridBenchmarkCases
    {
        protected override int ViewPortWidth => 15;
        protected override int ViewPortHeight => 15;
        protected override int ChunkWidth => 7;
        protected override int ChunkHeight => 7;
    }
}
