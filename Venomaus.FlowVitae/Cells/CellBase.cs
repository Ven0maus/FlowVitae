using Venomaus.FlowVitae.Grids;

namespace Venomaus.FlowVitae.Cells
{
    /// <summary>
    /// Base class which provides basic cell functionality
    /// </summary>
    /// <typeparam name="TCellType">The cell type to be used within the <see cref="GridBase{TCellType, TCell}"/></typeparam>
    public abstract class CellBase<TCellType> : ICell<TCellType>
        where TCellType : struct
    {
        /// <inheritdoc />
        public int X { get; set; }
        /// <inheritdoc />
        public int Y { get; set; }
        /// <inheritdoc />
        public TCellType CellType { get; set; }

        /// <summary>
        /// Constructor for <see cref="CellBase{TCellType}"/>
        /// </summary>
        public CellBase()
        { }

        /// <summary>
        /// Constructor for <see cref="CellBase{TCellType}"/>
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <param name="cellType">The cell type to be used within the <see cref="GridBase{TCellType, TCell}"/></param>
        public CellBase(int x, int y, TCellType cellType = default)
        {
            X = x;
            Y = y;
            CellType = cellType;
        }

        /// <inheritdoc/>
        public bool Equals(ICell<TCellType>? other)
        {
            return other != null && X == other.X && Y == other.Y;
        }

        /// <inheritdoc/>
        public bool Equals((int x, int y) other)
        {
            return X == other.x && Y == other.y;
        }
    }
}
