using Venomaus.FlowVitae.Basics.Chunking;
using Venomaus.FlowVitae.Basics.Procedural;

namespace Venomaus.FlowVitae.Basics
{
    /// <summary>
    /// Base class which provides basic grid functionality
    /// </summary>
    /// <typeparam name="TCellType">The cell type to be used within the <see cref="IGrid{TCellType, TCell}"/></typeparam>
    /// <typeparam name="TCell">The wrapper object used to wrap around the cell type</typeparam>
    public abstract class GridBase<TCellType, TCell> : IGrid<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        /// <summary>
        /// Seed that represents <see cref="GridBase{TCellType, TCell}"/>
        /// </summary>
        public int Seed { get; }
        /// <summary>
        /// Width of <see cref="Cells"/>
        /// </summary>
        public int Width { get; }
        /// <summary>
        /// Height of <see cref="Cells"/>
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Internal cell container for the defined cell type
        /// </summary>
        protected readonly TCellType[] Cells;
        /// <summary>
        /// Container for cells that contain extra data
        /// </summary>
        private Dictionary<(int x, int y), TCell>? _storage;
        /// <summary>
        /// Internal chunk manager object that handles chunk loading
        /// </summary>
        internal readonly ChunkLoader<TCellType, TCell>? _chunkLoader;

        /// <summary>
        /// Constructor for <see cref="GridBase{TCellType, TCell}"/>
        /// </summary>
        /// <remarks>
        /// When chunks are enabled <paramref name="width"/>, <paramref name="height"/> are used to determine the chunk size.
        /// </remarks>
        /// <param name="width">Width of <see cref="Cells"/></param>
        /// <param name="height">Height of <see cref="Cells"/></param>
        /// <param name="generator"></param>
        public GridBase(int width, int height, IProceduralGen<TCellType, TCell>? generator = null)
        {
            Width = width;
            Height = height;
            Cells = new TCellType[Width * Height];

            // Initialize chunkloader if grid uses chunks
            if (generator != null)
            {
                _chunkLoader = new ChunkLoader<TCellType, TCell>(Width, Height, generator, Convert);
                _chunkLoader.LoadChunksAround(0, 0, true);
            }
        }

        /// <summary>
        /// Converts the internal cell <typeparamref name="TCellType"/> to a readable cell of type <typeparamref name="TCell"/>
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <param name="cellType">Internal cell <typeparamref name="TCellType"/></param>
        /// <returns><typeparamref name="TCell"/></returns>
        public virtual TCell Convert(int x, int y, TCellType cellType)
        {
            return new TCell
            {
                X = x,
                Y = y,
                CellType = cellType
            };
        }

        /// <summary>
        /// Update the cell's <typeparamref name="TCellType"/> and other properties if state is stored)
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="storeState">Stores the <paramref name="cell"/> with all its properties and field values.
        /// This value is always true when the grid uses chunks.
        /// </param>
        public void SetCell(TCell cell, bool storeState = false)
        {
            int x = cell.X;
            int y = cell.Y;

            if (_chunkLoader == null && !InBounds(x, y)) return;

            var pos = _chunkLoader != null ? _chunkLoader.RemapChunkCoordinate(x, y) : (x, y);

            Cells[pos.y * Width + pos.x] = cell.CellType;

            // Storage or chunking
            if (!storeState && _chunkLoader == null && _storage != null)
            {
                _storage.Remove((x, y));
                if (_storage.Count == 0)
                    _storage = null;
            }
            else if (!storeState && _chunkLoader == null)
            {
                if (_storage == null)
                    _storage = new Dictionary<(int x, int y), TCell>();
                _storage[(x, y)] = cell;
            }
            else if (_chunkLoader != null)
                _chunkLoader.SetChunkCell(x, y, cell, storeState);
        }

        /// <summary>
        /// Update the <typeparamref name="TCellType"/> at position (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <param name="cellType"></param>
        /// <param name="storeState">Converts <paramref name="cellType"/> to <typeparamref name="TCell"/> and stores it with all its properties and field values.
        /// This value is always true when the grid uses chunks.
        /// </param>
        public void SetCell(int x, int y, TCellType cellType, bool storeState = false) 
            => SetCell(Convert(x, y, cellType), storeState);

        /// <summary>
        /// Retrieve the <typeparamref name="TCell"/> at position (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <returns><typeparamref name="TCell"/>?</returns>
        public TCell? GetCell(int x, int y)
        {
            if (_chunkLoader == null && !InBounds(x, y)) return default;
            if (_chunkLoader == null && _storage != null && _storage.TryGetValue((x, y), out TCell? cell))
                return cell;
            else if (_chunkLoader != null)
                return _chunkLoader.GetChunkCell(x, y);
            return Convert(x, y, Cells[y * Width + x]);
        }

        /// <summary>
        /// Retrieve the <typeparamref name="TCellType"/> at position (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <returns><typeparamref name="TCellType"/></returns>
        public TCellType GetCellType(int x, int y)
        {
            if (!InBounds(x, y)) return default;
            return Cells[y * Width + x];
        }

        /// <summary>
        /// Validation to see if the coordinate (<paramref name="x"/>, <paramref name="y"/>) is within the <see cref="Cells"/> bounds
        /// </summary>
        /// <remarks>Always true when chunking is used.</remarks>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <returns><see langword="true"/> or <see langword="false"/></returns>
        public bool InBounds(int x, int y)
        {
            if (_chunkLoader != null) return true;
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        /// <summary>
        /// Validation to see if the (<typeparamref name="TCell"/>.X, <typeparamref name="TCell"/>.Y) is within the <see cref="Cells"/> bounds
        /// </summary>
        /// <param name="cell"></param>
        /// <remarks>Always true when chunking is used.</remarks>
        /// <returns><see langword="true"/> or <see langword="false"/></returns>
        public bool InBounds(TCell cell)
        {
            if (_chunkLoader != null) return true;
            return InBounds(cell.X, cell.Y);
        }
    }
}
