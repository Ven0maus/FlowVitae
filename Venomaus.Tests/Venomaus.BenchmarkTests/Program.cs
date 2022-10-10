using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Venomaus.BenchmarkTests.Benchmarks.Cases.ProceduralGridCases;
using Venomaus.BenchmarkTests.Benchmarks.Cases.StaticGridCases;

namespace Venomaus.BenchmarkTests
{
    internal class Program
    {
        private static int ViewPortWidth = 0;
        private static int ViewPortHeight = 0;
        private static int ChunkWidth = 0;
        private static int ChunkHeight = 0;

        private static void Main(string[] args)
        {
            RunSmallGridBenchmarks();

            Console.WriteLine();
            Console.WriteLine("Benchmarking finished.");
            Console.ReadKey();
        }

        private static void RunSmallGridBenchmarks()
        {
            ViewPortWidth = 15;
            ViewPortHeight = 15;
            ChunkWidth = 15;
            ChunkHeight = 15;

            BenchmarkRunner.Run(new Type[]
            {
                typeof(StaticGridBenchmarkCases),
                typeof(ProcGenGridBenchmarkCases),
                typeof(ProcGenGridHalfChunkBenchmarkCases)
            }, ManualConfig
                .CreateMinimumViable()
                .WithOptions(ConfigOptions.DisableLogFile)
                .AddExporter(HtmlExporter.Default)
                .KeepBenchmarkFiles(false)
                .AddJob(Job.ShortRun));
        }

        public static BenchmarkSettings GetBenchmarkSettings()
        {
            return new BenchmarkSettings(ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight);
        }
    }
}