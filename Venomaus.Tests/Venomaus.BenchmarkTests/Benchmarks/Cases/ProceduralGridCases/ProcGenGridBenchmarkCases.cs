﻿using BenchmarkDotNet.Attributes;

namespace Venomaus.BenchmarkTests.Benchmarks.Cases.ProceduralGridCases
{
    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.SlowestToFastest)]
    public class ProcGenGridBenchmarkCases : ProcGenBaseBenchmarks
    {
        protected override bool DivideChunk => false;
    }
}
