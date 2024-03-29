﻿using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Venomaus.BenchmarkTests.Benchmarks.Cases.ProceduralGridCases;
using Venomaus.BenchmarkTests.Benchmarks.Cases.StaticGridCases;

namespace Venomaus.BenchmarkTests
{
    internal class Program
    {
        private static readonly Job JobType = Job.ShortRun;
        private const bool UseDefaultConfig = false;

        private static void Main()
        {
            CleanupLogging();

            // Run benchmarks on the defined size
            RunGridBenchmarks(viewPortWidth: 240, viewPortHeight: 67, chunkWidth: 32, chunkHeight: 32);

            Console.ReadKey();
        }

        private static Type[] BenchmarkCases()
        {
            return new Type[]
            {
                typeof(ChunkloaderBenchmarks),
                typeof(StaticGridBenchmarks),
                typeof(ProcGenGridBenchmarks)
            };
        }

        private static IConfig GetCustomConfig()
        {
            return ManualConfig
                .CreateMinimumViable()
                .WithOptions(ConfigOptions.DisableLogFile | ConfigOptions.JoinSummary)
                .AddExporter(HtmlExporter.Default)
                .KeepBenchmarkFiles(false)
                .AddJob(JobType);
        }

        private static IConfig GetDefaultConfig()
        {
            return DefaultConfig.Instance
                .WithOptions(ConfigOptions.DisableLogFile | ConfigOptions.JoinSummary)
                .AddExporter(HtmlExporter.Default)
                .KeepBenchmarkFiles(false);
        }

        public static void WriteLine(string text)
        {
            var path = "InitializationBenchmark.txt";
            var read = File.Exists(path) ? File.ReadAllText(path) : "";
            if (!string.IsNullOrEmpty(read))
                read += Environment.NewLine;
            read += text;
            File.WriteAllText(path, read);
        }

        private static void CleanupLogging()
        {
            var path = "InitializationBenchmark.txt";
            if (File.Exists(path))
                File.Delete(path);
        }

        private static void RunGridBenchmarks(int viewPortWidth, int viewPortHeight, int chunkWidth, int chunkHeight)
        {
            ViewPortWidth = viewPortWidth;
            ViewPortHeight = viewPortHeight;
            ChunkWidth = chunkWidth;
            ChunkHeight = chunkHeight;

            BenchmarkRunner.Run(BenchmarkCases(), UseDefaultConfig ? GetDefaultConfig() : GetCustomConfig());
        }

        private static int ViewPortWidth = 0;
        private static int ViewPortHeight = 0;
        private static int ChunkWidth = 0;
        private static int ChunkHeight = 0;

        public static BenchmarkSettings GetBenchmarkSettings()
        {
            return new BenchmarkSettings(ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight);
        }
    }
}