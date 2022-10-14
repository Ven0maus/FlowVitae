using System;
using System.Collections.Generic;

namespace Venomaus.FlowVitae.Basics.Chunking
{
    /// <summary>
    /// The arguments for when a chunk is loaded or unloaded
    /// </summary>
    public class ChunkUpdateArgs : EventArgs
    {
        /// <summary>
        /// Represents the most bottom-left X coordinate of the chunk
        /// </summary>
        public int ChunkX { get; }
        /// <summary>
        /// Represents the most bottom-left Y coordinate of the chunk
        /// </summary>
        public int ChunkY { get; }

        private readonly int _chunkSizeX, _chunkSizeY;

        /// <summary>
        /// Base constructor for <see cref="ChunkUpdateArgs"/>
        /// </summary>
        /// <param name="chunkCoordinate">The chunk coordinate</param>
        /// <param name="chunkSizeX">Chunk size X</param>
        /// <param name="chunkSizeY">Chunk size Y</param>
        internal ChunkUpdateArgs((int x, int y) chunkCoordinate, int chunkSizeX, int chunkSizeY)
        {
            ChunkX = chunkCoordinate.x;
            ChunkY = chunkCoordinate.y;
            _chunkSizeX = chunkSizeX;
            _chunkSizeY = chunkSizeY;
        }

        /// <summary>
        /// Returns all world cell positions of this chunk
        /// </summary>
        /// <returns>An enumerable of (x,y) positions that are contained within the chunk</returns>
        public IEnumerable<(int x, int y)> GetCellPositions()
        {
            for (int x = 0; x < _chunkSizeX; x++)
            {
                for (int y = 0; y < _chunkSizeY; y++)
                {
                    yield return (ChunkX + x, ChunkY + y);
                }
            }
        }
    }
}
