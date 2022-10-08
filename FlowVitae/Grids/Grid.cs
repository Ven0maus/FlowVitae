using Venomaus.FlowVitae.Basics;
using Venomaus.FlowVitae.Basics.Procedural;

namespace Venomaus.FlowVitae.Grids
{
    /// <summary>
    /// A basic high-performance memory efficient grid implementation
    /// </summary>
    /// <inheritdoc />
    public sealed class Grid<TCellType, TCell> : GridBase<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        private Func<int, int, TCellType, TCell>? _customConverter;

        /// <inheritdoc />
        public Grid(int width, int height) : base(width, height)
        {
        }

        /// <inheritdoc />
        public Grid(int viewPortWidth, int viewPortHeight, int chunkWidth, int chunkHeight, IProceduralGen<TCellType, TCell>? generator) : base(viewPortWidth, viewPortHeight, chunkWidth, chunkHeight, generator)
        {
        }

        /// <inheritdoc />
        public Grid(int viewPortWidth, int viewPortHeight, IProceduralGen<TCellType, TCell>? generator) : base(viewPortWidth, viewPortHeight, generator)
        {
        }

        /// <summary>
        /// Overwrites the Convert method with a custom implementation without having to create a new <see cref="GridBase{TCellType, TCell}"/> implementation.
        /// </summary>
        /// <param name="converter">Converter func that resembles the Convert method</param>
        public void SetCustomConverter(Func<int, int, TCellType, TCell>? converter)
        {
            _customConverter = converter;
        }

        /// <inheritdoc />
        protected override TCell Convert(int x, int y, TCellType cellType)
        {
            return _customConverter != null ? _customConverter.Invoke(x, y, cellType) : 
                base.Convert(x, y, cellType);
        }
    }
}