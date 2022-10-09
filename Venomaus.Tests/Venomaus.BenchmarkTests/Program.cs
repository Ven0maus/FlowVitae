using BenchmarkDotNet.Running;
using Venomaus.BenchmarkTests.Benchmarks;

namespace Venomaus.BenchmarkTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run<GridBenchmarks>();

            Console.WriteLine();
            Console.WriteLine("Benchmarking finished.");
            Console.ReadKey();
        }
    }
}