using Venomaus.FlowVitae.Basics;

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
        /// <inheritdoc />
        public Grid(int width, int height) : base(width, height)
        { }
    }
}