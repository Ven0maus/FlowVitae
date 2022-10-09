using BenchmarkDotNet.Running;
using Venomaus.BenchmarkTests.Benchmarks.Configurations.ProceduralGrids.DifferentChunkSize;
using Venomaus.BenchmarkTests.Benchmarks.Configurations.ProceduralGrids.SameChunkSize;
using Venomaus.BenchmarkTests.Benchmarks.Configurations.StaticGrids;

namespace Venomaus.BenchmarkTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run(new Type[]
            {
                typeof(SmallStaticGridBenchmarks),
                typeof(SmallSSProcGenBenchmarks),
                typeof(SmallDSProcGenBenchmarks)
            });

            Console.WriteLine();
            Console.WriteLine("Benchmarking finished.");
            Console.ReadKey();
        }
    }
}