using Venomaus.BenchmarkTests.Benchmarks.Cases.ProceduralGridCases;

namespace Venomaus.BenchmarkTests.Benchmarks.Configurations.ProceduralGrids.SameChunkSize
{
    internal class SmallSSProcGenBenchmarks : ProcGenGridBenchmarkCases
    {
        protected override int ViewPortWidth => 15;
        protected override int ViewPortHeight => 15;
        protected override int ChunkWidth => 15;
        protected override int ChunkHeight => 15;
    }
}
