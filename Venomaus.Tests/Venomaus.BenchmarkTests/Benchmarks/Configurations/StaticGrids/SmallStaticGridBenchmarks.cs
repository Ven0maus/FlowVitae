using Venomaus.BenchmarkTests.Benchmarks.Cases.StaticGridCases;

namespace Venomaus.BenchmarkTests.Benchmarks.Configurations.StaticGrids
{
    public class SmallStaticGridBenchmarks : StaticGridBenchmarkCases
    {
        protected override int ViewPortWidth => 15;
        protected override int ViewPortHeight => 15;
        protected override int ChunkWidth => 15;
        protected override int ChunkHeight => 15;
    }
}
