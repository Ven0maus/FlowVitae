using Venomaus.FlowVitae.Basics;

namespace Venomaus.FlowVitae.Cells
{
    /// <summary>
    /// A basic generic implementation of <see cref="ICell{TCellType}"/>
    /// </summary>
    /// <remarks>Contains some default properties, in most cases <see cref="CellBase{TCellType}"/> can be used to create your own Cell.</remarks>
    /// <inheritdoc />
    public sealed class Cell<TCellType> : CellBase<TCellType>
        where TCellType : struct
    {
        /// <summary>
        /// Determines if the cell can be stepped on
        /// </summary>
        public bool Walkable { get; set; } = true;

        /// <inheritdoc />
        public Cell()
        { }

        /// <inheritdoc />
        public Cell(int x, int y) : base(x, y, default)
        { }

        /// <inheritdoc />
        public Cell(int x, int y, TCellType cellType) : base(x, y, cellType)
        { }

        /// <inheritdoc />
        public Cell(int x, int y, bool walkable, TCellType cellType) : base(x, y, cellType)
        {
            Walkable = walkable;
        }
    }
}
