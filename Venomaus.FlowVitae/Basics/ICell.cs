using System;

namespace Venomaus.FlowVitae.Basics
{
    /// <summary>
    /// Interface for a <typeparamref name="TCellType"/> wrapper used in <see cref="IGrid{TCellType, TCell}"/>
    /// </summary>
    /// <typeparam name="TCellType">The cell type for a <see cref="IGrid{TCellType, TCell}"/></typeparam>
    public interface ICell<TCellType> : IEquatable<ICell<TCellType>>, IEquatable<(int x, int y)>
        where TCellType : struct
    {
        /// <summary>
        /// Coordinate X
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Coordinate Y
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// Cell type
        /// </summary>
        public TCellType CellType { get; set; }
    }
}
