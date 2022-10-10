using BenchmarkDotNet.Attributes;

namespace Venomaus.BenchmarkTests.Benchmarks.Cases.ProceduralGridCases
{
    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.SlowestToFastest)]
    public class ProcGenGridDivideBenchmarkCases : ProcGenBaseBenchmarks
    {
        protected override bool DivideChunk => true;
    }
}
