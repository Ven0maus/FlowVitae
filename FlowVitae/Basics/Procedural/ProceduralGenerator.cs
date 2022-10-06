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

        private readonly Func<Random, TCellType> _initializer;

        /// <summary>
        /// Constructor for <see cref="ProceduralGenerator{TCellType, TCell}"/>
        /// </summary>
        /// <param name="seed">Unique seed</param>
        /// <param name="initializer"><typeparamref name="TCellType"/> initializer</param>
        public ProceduralGenerator(int seed, Func<Random, TCellType> initializer)
        {
            Seed = seed;
            _initializer = initializer;
        }

        /// <inheritdoc />
        public TCellType[] Generate(int seed, int width, int height)
        {
            var random = new Random(seed);
            var grid = new TCellType[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[y * width + x] = _initializer.Invoke(random);
                }
            }
            return grid;
        }
    }
}
