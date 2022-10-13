using Venomaus.FlowVitae.Basics.Chunking;

namespace Venomaus.UnitTests.Tools
{
    internal class TestChunkData : IChunkData
    {
        public int Seed { get; set; }
        public List<(int x, int y)>? Trees { get; set; }
        (int x, int y) IChunkData.ChunkCoordinate { get; set; }
    }
}
