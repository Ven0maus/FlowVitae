namespace Venomaus.FlowVitae.Basics.Chunking
{
    /// <summary>
    /// The base interface for the chunk data container
    /// </summary>
    public interface IChunkData
    {
        /// <summary>
        /// The unique chunk seed
        /// </summary>
        public int Seed { get; set; }
    }
}
