using BenchmarkDotNet.Attributes;

namespace Venomaus.BenchmarkTests.Benchmarks.Cases.ProceduralGridCases
{
    [MemoryDiagnoser]
    public class ProcGenGridDivideBenchmarkCases : ProcGenBaseBenchmarks
    {
        protected override bool DivideChunk => true;
    }
}
