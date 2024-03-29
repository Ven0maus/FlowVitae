﻿using System.Collections.Generic;
using System.Linq;
using Venomaus.FlowVitae.Helpers;

namespace Venomaus.FlowVitae.Chunking
{
    /// <summary>
    /// Container for chunk information retrieved from the grid
    /// </summary>
    public sealed class ChunkLoadInformation
    {
        /// <summary>
        /// Contains all chunk coordinates that are situated in the current viewport
        /// </summary>
        public IReadOnlyList<(int x, int y)> ChunksInsideViewport { get; }
        /// <summary>
        /// Contains all chunk coordinates that are not in the current viewport
        /// </summary>
        public IReadOnlyList<(int x, int y)> ChunksOutsideViewport { get; }
        /// <summary>
        /// Contains a concatenation of ChunksInsideViewport and ChunksOutsideViewport
        /// </summary>
        public IReadOnlyList<(int x, int y)> AllChunks { get; }

        internal ChunkLoadInformation(List<(int x, int y)> chunksInsideViewPort, List<(int x, int y)> chunksOutsideViewport)
        {
            ChunksInsideViewport = chunksInsideViewPort;
            ChunksOutsideViewport = chunksOutsideViewport;
            AllChunks = chunksInsideViewPort.Concat(chunksOutsideViewport).ToList();
        }

        internal ChunkLoadInformation GetDifference(ChunkLoadInformation information)
        {
            var comparer = new TupleComparer<int>();
            return new ChunkLoadInformation(
                ChunksInsideViewport.Except(information.ChunksInsideViewport, comparer).ToList(),
                ChunksOutsideViewport.Except(information.ChunksOutsideViewport, comparer).ToList());
        }

        internal static (IEnumerable<(int x, int y)> addedChunks, IEnumerable<(int x, int y)> removedChunks)
            GetAddedRemovedChunks(ChunkLoadInformation initial, ChunkLoadInformation updated)
        {
            var addedChunksInsideViewport = updated.ChunksInsideViewport
                .Except(initial.ChunksInsideViewport, new TupleComparer<int>())
                .ToList();
            var addedChunksOutsideViewport = updated.ChunksOutsideViewport
                .Except(initial.ChunksOutsideViewport, new TupleComparer<int>())
                .ToList();
            var removedChunksInsideViewport = initial.ChunksInsideViewport
                .Except(updated.ChunksInsideViewport, new TupleComparer<int>())
                .ToList();
            var removedChunksOutsideViewport = initial.ChunksOutsideViewport
                .Except(updated.ChunksOutsideViewport, new TupleComparer<int>())
                .ToList();

            var addedChunks = addedChunksInsideViewport.Concat(addedChunksOutsideViewport);
            var removedChunks = removedChunksInsideViewport.Concat(removedChunksOutsideViewport);
            return (addedChunks, removedChunks);
        }
    }
}
