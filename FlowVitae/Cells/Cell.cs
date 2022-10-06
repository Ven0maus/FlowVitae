using Venomaus.FlowVitae.Basics;

namespace Venomaus.FlowVitae.Cells
{
    /// <summary>
    /// A basic generic implementation of <see cref="ICell{TCellType}"/>
    /// </summary>
    /// <inheritdoc />
    public class Cell<TCellType> : CellBase<TCellType>
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
