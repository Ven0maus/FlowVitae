namespace Venomaus.BenchmarkTests
{
    public class BenchmarkSettings
    {
        public int ViewPortWidth { get; set; }
        public int ViewPortHeight { get; set; }
        public int ChunkWidth { get; set; }
        public int ChunkHeight { get; set; }

        public BenchmarkSettings(int viewPortWidth, int viewPortHeight, int chunkWidth, int chunkHeight)
        {
            ViewPortWidth = viewPortWidth;
            ViewPortHeight = viewPortHeight;
            ChunkWidth = chunkWidth;
            ChunkHeight = chunkHeight;
        }
    }
}