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
        private readonly Lazy<Dictionary<(int x, int y), TCell>> _storage;
        /// <summary>
        /// Internal chunk manager object that handles chunk loading
        /// </summary>
        private readonly ChunkLoader<TCellType, TCell>? _chunkLoader;

        /// <summary>
        /// Constructor for <see cref="GridBase{TCellType, TCell}"/>
        /// </summary>
        /// <remarks>
        /// When chunks are enabled <paramref name="width"/>, <paramref name="height"/> are used to determine the chunk size.
        /// </remarks>
        /// <param name="width">Width of <see cref="Cells"/></param>
        /// <param name="height">Height of <see cref="Cells"/></param>
        /// <param name="seed">Seed used by <see cref="ChunkLoader{TCellType, TCell}"/></param>
        /// <param name="generator"></param>
        public GridBase(int width, int height, int seed = 0, IProceduralGen<TCellType, TCell>? generator = null)
        {
            Width = width;
            Height = height;
            Seed = seed;
            Cells = new TCellType[Width * Height];

            // Initialize lazy objects
            _storage = new Lazy<Dictionary<(int x, int y), TCell>>
                (() => new Dictionary<(int x, int y), TCell>());

            // Initialize chunkloader if grid uses chunks
            if (generator != null)
                _chunkLoader = new ChunkLoader<TCellType, TCell>(Width, Height, Seed, generator);
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
        /// Update the cell's <typeparamref name="TCellType"/> at position (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <param name="cell"></param>
        /// <param name="storeState">Stores the <paramref name="cell"/> with all its properties and field values</param>
        public void SetCell(int x, int y, TCell cell, bool storeState = false)
        {
            if (!InBounds(x, y)) return;

            TCellType cellType;
            if (cell == null)
            {
                cellType = default;
                var key = (x, y);
                if (_storage.IsValueCreated && _storage.Value.ContainsKey(key))
                {
                    _storage.Value.Remove(key);
                }
            }
            else
            {
                cellType = cell.CellType;
            }

            Cells[y * Width + x] = cellType;

            if (storeState && cell != null && InBounds(x, y))
                _storage.Value[(x, y)] = cell;   
        }

        /// <summary>
        /// Update the <typeparamref name="TCellType"/> at position (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <param name="cellType"></param>
        public void SetCell(int x, int y, TCellType cellType)
        {
            if (!InBounds(x, y)) return;
            Cells[y * Width + x] = cellType;
        }

        /// <summary>
        /// Retrieve the <typeparamref name="TCell"/> at position (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <returns><typeparamref name="TCell"/>?</returns>
        public TCell? GetCell(int x, int y)
        {
            if (!InBounds(x, y)) return default;
            return _storage.IsValueCreated && _storage.Value.TryGetValue((x, y), out TCell? cell) ? 
                cell : Convert(x, y, Cells[y * Width + x]);
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
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <returns><see langword="true"/> or <see langword="false"/></returns>
        public bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        /// <summary>
        /// Validation to see if the (<typeparamref name="TCell"/>.X, <typeparamref name="TCell"/>.Y) is within the <see cref="Cells"/> bounds
        /// </summary>
        /// <param name="cell"></param>
        /// <returns><see langword="true"/> or <see langword="false"/></returns>
        public bool InBounds(TCell cell)
        {
            return InBounds(cell.X, cell.Y);
        }
    }
}
