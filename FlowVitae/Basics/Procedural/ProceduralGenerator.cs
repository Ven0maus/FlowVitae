namespace Venomaus.FlowVitae.Basics.Procedural
{
    /// <summary>
    /// Basic procedural generator implementation
    /// </summary>
    /// <typeparam name="TCell">The wrapper object used to wrap around the cell type</typeparam>
    public sealed class ProceduralGenerator<TCell> : IProceduralGen<int, TCell>
        where TCell : class, ICell<int>, new()
    {
        /// <inheritdoc />
        public int Seed { get; private set; }

        /// <summary>
        /// Constructor for <see cref="ProceduralGenerator{intTCell}"/>
        /// </summary>
        /// <param name="seed">Unique seed</param>
        public ProceduralGenerator(int seed)
        {
            Seed = seed;
        }

        /// <inheritdoc />
        public int[] Generate(int seed, int width, int height)
        {
            var random = new Random(seed);
            var grid = new int[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[y * width + x] = random.Next(0, 10);
                }
            }
            return grid;
        }
    }
}
