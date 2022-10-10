using BenchmarkDotNet.Attributes;

namespace Venomaus.BenchmarkTests.Benchmarks.Cases.ProceduralGridCases
{
    [MemoryDiagnoser]
    public class ProcGenGridBenchmarkCases : ProcGenBaseBenchmarks
    {
        protected override bool DivideChunk => false;
    }
}
