using System;
using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Chunking;

namespace Venomaus.FlowVitae.Generators
{
    /// <summary>
    /// Basic procedural generator implementation
    /// </summary>
    /// <typeparam name="TCellType">The cell type to be used within the <see cref="IProceduralGen{TCellType, TCell}"/></typeparam>
    /// <typeparam name="TCell">The wrapper object used to wrap around the cell type</typeparam>
    public sealed class ProceduralGenerator<TCellType, TCell> : IProceduralGen<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        /// <inheritdoc />
        public int Seed { get; }

        private readonly Action<Random, TCellType[], int, int, (int x, int y)> _method;

        /// <summary>
        /// Basic procedural algorithm, method param uses following signature: (<see cref="Random"/>, <typeparamref name="TCellType"/>[], width, height)
        /// </summary>
        /// <param name="seed">Unique seed</param>
        /// <param name="method">Signature: (<see cref="Random"/>, <typeparamref name="TCellType"/>[], width, height)</param>
        public ProceduralGenerator(int seed, Action<Random, TCellType[], int, int, (int x, int y)> method)
        {
            Seed = seed;
            _method = method;
        }

        /// <inheritdoc />
        public (TCellType[] chunkCells, IChunkData? chunkData) Generate(int seed, int width, int height, (int x, int y) chunkCoordinate)
        {
            var random = new Random(seed);
            var grid = new TCellType[width * height];

            // Custom generation method
            _method.Invoke(random, grid, width, height, chunkCoordinate);

            return (grid, null);
        }
    }

    /// <summary>
    /// Basic procedural generator implementation
    /// </summary>
    /// <typeparam name="TCellType">The cell type to be used within the <see cref="IProceduralGen{TCellType, TCell}"/></typeparam>
    /// <typeparam name="TCell">The wrapper object used to wrap around the cell type</typeparam>
    /// <typeparam name="TChunkData">The chunk data container object</typeparam>
    public sealed class ProceduralGenerator<TCellType, TCell, TChunkData> : IProceduralGen<TCellType, TCell, TChunkData>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
        where TChunkData : class, IChunkData
    {
        /// <inheritdoc />
        public int Seed { get; private set; }

        private readonly Func<Random, TCellType[], int, int, (int x, int y), TChunkData> _method;

        /// <summary>
        /// Basic procedural algorithm, method param uses following signature: (<see cref="Random"/>, <typeparamref name="TCellType"/>[], width, height, chunkCoordinate)
        /// </summary>
        /// <param name="seed">Unique seed</param>
        /// <param name="method">Signature: (<see cref="Random"/>, <typeparamref name="TCellType"/>[], width, height)</param>
        public ProceduralGenerator(int seed, Func<Random, TCellType[], int, int, (int x, int y), TChunkData> method)
        {
            Seed = seed;
            _method = method;
        }

        /// <inheritdoc />
        public (TCellType[] chunkCells, TChunkData? chunkData) Generate(int seed, int width, int height, (int x, int y) chunkCoordinate)
        {
            var random = new Random(seed);
            var grid = new TCellType[width * height];

            // Custom generation method
            var chunkData = _method.Invoke(random, grid, width, height, chunkCoordinate);

            return (grid, chunkData);
        }
    }
}
