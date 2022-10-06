using Venomaus.FlowVitae.Basics;

namespace Venomaus.FlowVitae.Cells
{
    /// <summary>
    /// A basic <see langword="int"/> implementation of <see cref="ICell{TCellType}"/>
    /// </summary>
    /// <inheritdoc />
    public sealed class Cell : CellBase<int>
    {
        /// <inheritdoc />
        public Cell()
        { }

        /// <inheritdoc />
        public Cell(int x, int y, int cellType = default) : base(x, y, cellType)
        { }
    }

    /// <summary>
    /// A basic generic implementation of <see cref="ICell{TCellType}"/>
    /// </summary>
    /// <inheritdoc />
    public sealed class Cell<TCellType> : CellBase<TCellType>
        where TCellType : struct
    {
        /// <inheritdoc />
        public Cell()
        { }

        /// <inheritdoc />
        public Cell(int x, int y, TCellType cellType = default) : base(x, y, cellType)
        { }
    }
}
