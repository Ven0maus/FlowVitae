using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
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
            RunSmallBenchmarks();

            Console.WriteLine();
            Console.WriteLine("Benchmarking finished.");
            Console.ReadKey();
        }

        private static void RunSmallBenchmarks()
        {
            BenchmarkRunner.Run(new Type[]
            {
                typeof(SmallStaticGridBenchmarks),
                typeof(SmallSSProcGenBenchmarks),
                typeof(SmallDSProcGenBenchmarks)
            }, ManualConfig
                .CreateMinimumViable()
                .WithOptions(ConfigOptions.DisableLogFile)
                .AddExporter(HtmlExporter.Default)
                .KeepBenchmarkFiles(false)
                .AddJob(Job.ShortRun));
        }
    }
}