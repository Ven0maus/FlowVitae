using System;

namespace Venomaus.FlowVitae.Basics.Procedural
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
        public int Seed { get; private set; }

        private readonly Action<Random, TCellType[], int, int> _method;

        /// <summary>
        /// Basic procedural algorithm, method param uses following signature: (<see cref="Random"/>, <typeparamref name="TCellType"/>[], width, height)
        /// </summary>
        /// <param name="seed">Unique seed</param>
        /// <param name="method">Signature: (<see cref="Random"/>, <typeparamref name="TCellType"/>[], width, height)</param>
        public ProceduralGenerator(int seed, Action<Random, TCellType[], int, int> method)
        {
            Seed = seed;
            _method = method;
        }

        /// <inheritdoc />
        public TCellType[] Generate(int seed, int width, int height)
        {
            var random = new Random(seed);
            var grid = new TCellType[width * height];

            // Custom generation method
            _method.Invoke(random, grid, width, height);

            return grid;
        }
    }
}
