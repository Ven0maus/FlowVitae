using BenchmarkDotNet.Attributes;
using Venomaus.FlowVitae.Cells;

namespace Venomaus.BenchmarkTests.Benchmarks.Cases.ProceduralGridCases
{
    [MemoryDiagnoser]
    public class ChunkloaderBenchmarks : BaseGridBenchmarks<int, Cell<int>>
    {
        protected override int Seed => 1000;
        protected override bool ProcGenEnabled => true;

        protected override void GenerateChunk(Random random, int[] chunk, int width, int height, (int x, int y) chunkCoordinate)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    chunk[y * width + x] = random.Next(-10, 10);
        }

        [Benchmark]
        public bool UnloadChunk()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8629 // Nullable value type may be null.
            return Grid._chunkLoader.UnloadChunk(CurrentChunkCoordinate.Value.x, CurrentChunkCoordinate.Value.y, true);
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [Benchmark]
        public bool LoadChunk_Threaded_Parallel()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Grid.LoadChunksInParallel = true;
            Grid.UseThreading = true;
            return Grid._chunkLoader.LoadChunk(UnloadedChunkCoordinate.x, UnloadedChunkCoordinate.y, out _);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [Benchmark]
        public bool LoadChunk_Threaded_NonParallel()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Grid.LoadChunksInParallel = false;
            Grid.UseThreading = true;
            return Grid._chunkLoader.LoadChunk(UnloadedChunkCoordinate.x, UnloadedChunkCoordinate.y, out _);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [Benchmark]
        public bool LoadChunk_NonThreaded_Parallel()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Grid.LoadChunksInParallel = true;
            Grid.UseThreading = false;
            return Grid._chunkLoader.LoadChunk(UnloadedChunkCoordinate.x, UnloadedChunkCoordinate.y, out _);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [Benchmark]
        public bool LoadChunk_NonThreaded_NonParallel()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Grid.LoadChunksInParallel = false;
            Grid.UseThreading = false;
            return Grid._chunkLoader.LoadChunk(UnloadedChunkCoordinate.x, UnloadedChunkCoordinate.y, out _);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
    }
}
