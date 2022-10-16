using Venomaus.FlowVitae.Chunking;

namespace Venomaus.UnitTests.Tools
{
    internal class TestChunkData : IChunkData
    {
        public int Seed { get; set; }
        public HashSet<(int x, int y)>? Trees { get; set; }
        (int x, int y) IChunkData.ChunkCoordinate { get; set; }
    }
}
