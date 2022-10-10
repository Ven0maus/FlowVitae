using System;

namespace Venomaus.FlowVitae.Basics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TCellType"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    public class CellUpdateArgs<TCellType, TCell> : EventArgs
        where TCellType : struct
        where TCell : ICell<TCellType>, new()
    {
        /// <summary>
        /// The X coordinate that represent the cell on the screen
        /// </summary>
        /// <remarks>This is different from the <see cref="Cell"/>.X</remarks>
        public int ScreenX { get; private set; }
        /// <summary>
        /// The Y coordinate that represent the cell on the screen
        /// </summary>
        /// <remarks>This is different from the <see cref="Cell"/>.Y</remarks>
        public int ScreenY { get; private set; }
        /// <summary>
        /// The updated <typeparamref name="TCell"/>
        /// </summary>
        public TCell Cell { get; private set; }

        internal CellUpdateArgs((int x, int y) screenCoordinate, TCell cell)
        {
            ScreenX = screenCoordinate.x;
            ScreenY = screenCoordinate.y;
            Cell = cell;
        }
    }
}