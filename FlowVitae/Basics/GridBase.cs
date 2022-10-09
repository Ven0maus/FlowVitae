using Venomaus.FlowVitae.Basics.Chunking;
using Venomaus.FlowVitae.Basics.Procedural;
using Venomaus.FlowVitae.Helpers;

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
        /// Width of <see cref="ScreenCells"/>
        /// </summary>
        public int Width { get; }
        /// <summary>
        /// Height of <see cref="ScreenCells"/>
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Internal cell container for the defined cell type
        /// </summary>
        internal readonly TCellType[] ScreenCells;

        /// <summary>
        /// Represents the real coordinate that is located in the center of the screen cells.
        /// </summary>
        private (int x, int y) _centerCoordinate;
        /// <summary>
        /// A custom converter method, to replace the default cell converter
        /// </summary>
        private Func<int, int, TCellType, TCell>? _customConverter;
        /// <summary>
        /// Container for cells that contain extra data
        /// </summary>
        private Dictionary<(int x, int y), TCell>? _storage;
        /// <summary>
        /// Internal chunk manager object that handles chunk loading
        /// </summary>
        internal readonly ChunkLoader<TCellType, TCell>? _chunkLoader;

        /// <summary>
        /// Raised every time a screen cell is updated, can be used for rendering updates.
        /// </summary>
        /// <remarks>See <see cref="RaiseOnlyOnCellTypeChange"/> to control how this event is raised.</remarks>
        public event EventHandler<CellUpdateArgs<TCellType, TCell>>? OnCellUpdate;

        private bool _raiseOnlyOnCellTypeChange = true;
        /// <summary>
        /// When false, it will always raise when the cell is set within the viewport, default true.
        /// </summary>
        /// <remarks>This can be useful when you require the full cell data when it is set on the <see cref="OnCellUpdate"/> event, even when the cell type does not change.</remarks>
        public bool RaiseOnlyOnCellTypeChange
        {
            get
            {
                return _raiseOnlyOnCellTypeChange;
            }
            set
            {
                _raiseOnlyOnCellTypeChange = value;
                if (_chunkLoader != null)
                    _chunkLoader.RaiseOnlyOnCellTypeChange = value;
            }
        }

        /// <summary>
        /// Constructor for <see cref="GridBase{TCellType, TCell}"/>
        /// </summary>
        /// <remarks>Initializes a grid that does not use chunking.</remarks>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public GridBase(int width, int height)
        {
            Width = width;
            Height = height;
            ScreenCells = new TCellType[Width * Height];
            _centerCoordinate = (Width / 2, Height / 2);
        }

        /// <summary>
        /// Constructor for <see cref="GridBase{TCellType, TCell}"/>
        /// </summary>
        /// <remarks>Initializes a grid that uses chunking. (<paramref name="chunkWidth"/>, <paramref name="chunkHeight"/>) are used to determine the chunk size.</remarks>
        /// <param name="viewPortWidth">Width of <see cref="ScreenCells"/>, this is what is usually rendered on screen.</param>
        /// <param name="viewPortHeight">Height of <see cref="ScreenCells"/>, this is what is usually rendered on screen.</param>
        /// <param name="chunkWidth">The width of the chunk</param>
        /// <param name="chunkHeight">The height of the chunk</param>
        /// <param name="generator">The procedural algorithm used to generate the chunk data</param>
        public GridBase(int viewPortWidth, int viewPortHeight, int chunkWidth, int chunkHeight, IProceduralGen<TCellType, TCell>? generator)
            : this (viewPortWidth, viewPortHeight)
        {
            if (generator == null) return;

            // Initialize chunkloader if grid uses chunks
            _chunkLoader = new ChunkLoader<TCellType, TCell>(chunkWidth, chunkHeight, generator, Convert);
            _chunkLoader.LoadChunksAround(0, 0, true);

            // By default center on the middle of the viewport
            // This helps reduce chunks when the chunk width/height is the same as viewport
            Center(viewPortWidth / 2, viewPortHeight / 2);
        }

        /// <summary>
        /// Constructor for <see cref="GridBase{TCellType, TCell}"/>
        /// </summary>
        /// <remarks>
        /// Initializes a grid that uses chunking. (<paramref name="viewPortWidth"/>, <paramref name="viewPortHeight"/>) are used to determine the chunk size.
        /// </remarks>
        /// <param name="viewPortWidth">Width of <see cref="ScreenCells"/></param>
        /// <param name="viewPortHeight">Height of <see cref="ScreenCells"/></param>
        /// <param name="generator">The procedural algorithm used to generate the chunk data</param>
        public GridBase(int viewPortWidth, int viewPortHeight, IProceduralGen<TCellType, TCell>? generator)
            : this(viewPortWidth, viewPortHeight, viewPortWidth, viewPortHeight, generator)
        { }

        /// <summary>
        /// Overwrites the Convert method with a custom implementation without having to create a new <see cref="GridBase{TCellType, TCell}"/> implementation.
        /// </summary>
        /// <param name="converter">Converter func that resembles the Convert method</param>
        public void SetCustomConverter(Func<int, int, TCellType, TCell>? converter)
        {
            _customConverter = converter;
        }

        /// <summary>
        /// Centers the grid on the specified coordinate
        /// </summary>
        /// <remarks>Can only be used for grids that use chunking.</remarks>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        public void Center(int x, int y)
        {
            if (_chunkLoader == null) return;

            _centerCoordinate = (x, y);

            var minX = x - (Width / 2);
            var minY = y - (Height / 2);

            // Update the current chunk
            var centerChunk = _chunkLoader.GetChunkCoordinate(x, y);
            _chunkLoader.SetCurrentChunk(centerChunk.x, centerChunk.y);

            // Collect all positions
            var positions = new (int, int)[Width * Height]; 
            for (var xX = 0; xX < Width; xX++)
            {
                for (var yY = 0; yY < Height; yY++)
                {
                    var cellX = minX + xX;
                    var cellY = minY + yY;
                    positions[yY * Width + xX] = (cellX, cellY);
                }
            }

            // Set cells properly to cell type
            var cells = GetCells(positions);
            foreach (var cell in cells)
            {
                var screenCoordinate = WorldToScreenCoordinate(cell.X, cell.Y);
                ScreenCells[screenCoordinate.y * Width + screenCoordinate.x] = cell.CellType;
                OnCellUpdate?.Invoke(null, new CellUpdateArgs<TCellType, TCell>(screenCoordinate, cell));
            }
        }

        /// <summary>
        /// Converts the internal cell <typeparamref name="TCellType"/> to a readable cell of type <typeparamref name="TCell"/>
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <param name="cellType">Internal cell <typeparamref name="TCellType"/></param>
        /// <returns><typeparamref name="TCell"/></returns>
        protected virtual TCell Convert(int x, int y, TCellType cellType)
        {
            return _customConverter != null ? _customConverter.Invoke(x, y, cellType) : new TCell
            {
                X = x,
                Y = y,
                CellType = cellType
            };
        }

        /// <summary>
        /// Returns the world coordinate that resembles the given screen coordinate.
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <returns><see cref="ValueTuple{Int32, Int32}"/></returns>
        internal (int x, int y) ScreenToWorldCoordinate(int x, int y)
        {
            if (!InBounds(x, y)) 
                throw new Exception("Invalid screen coordinate, must be within screen bounds (Width, Height).");
            
            int minX = _centerCoordinate.x - Width / 2;
            int minY = _centerCoordinate.y - Height / 2;

            return (minX + x, minY + y);
        }

        /// <summary>
        /// Returns the screen coordinate that resembles the given world coordinate.
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <remarks>Only useful when using a chunkloaded grid, static grids will just return input coordinate.</remarks>
        /// <returns><see cref="ValueTuple{Int32, Int32}"/></returns>
        internal (int x, int y) WorldToScreenCoordinate(int x, int y)
        {
            if (_chunkLoader == null)
            {
                if (!InBounds(x, y))
                    throw new Exception("Invalid world coordinate, must be within screen bounds (Width * Height).");
                return (x, y);
            }
            var halfCenterX = _centerCoordinate.x - (Width / 2);
            var halfCenterY = _centerCoordinate.y - (Height / 2);
            var modifiedPos = (x:  x - halfCenterX, y: y - halfCenterY);
            return modifiedPos;
        }

        /// <summary>
        /// Get a cloned version of the viewport cells where each cell coordinate is adjusted to the viewport.
        /// </summary>
        /// <remarks>Cell (x, y) are adjusted to match the viewport (x, y).</remarks>
        /// <returns><typeparamref name="TCell"/>[]</returns>
        public TCell[] GetViewPortCells()
        {
            var positions = new (int, int)[Width * Height];
            for (int x=0; x < Width; x++)
            {
                for (int y=0; y < Height; y++)
                {
                    positions[y * Width + x] = ScreenToWorldCoordinate(x, y);
                }
            }

            var cells = GetCells(positions).ToArray();
            foreach (var cell in cells)
            {
                var (x, y) = WorldToScreenCoordinate(cell.X, cell.Y);
                cell.X = x;
                cell.Y = y;
            }
            return cells;
        }

        /// <summary>
        /// Update the cell's <typeparamref name="TCellType"/> and other properties if state is stored)
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="storeState">Stores the <paramref name="cell"/> with all its properties and field values.
        /// This value is always true when the grid uses chunks.
        /// </param>
        /// <remarks>When storeState is <see langword="false"/>, and <typeparamref name="TCellType"/> is modified <see cref="OnCellUpdate"/> will be raised.
        /// When storeState is <see langword="true"/> <see cref="OnCellUpdate"/> will always be raised.</remarks>
        public void SetCell(TCell cell, bool storeState = false)
        {
            int x = cell.X;
            int y = cell.Y;

            if (_chunkLoader == null && !InBounds(x, y)) return;
            if (_chunkLoader == null)
            {
                var screenCoordinate = WorldToScreenCoordinate(x, y);
                var prev = ScreenCells[screenCoordinate.y * Width + screenCoordinate.x];
                ScreenCells[screenCoordinate.y * Width + screenCoordinate.x] = cell.CellType;

                if (!RaiseOnlyOnCellTypeChange || !prev.Equals(cell.CellType))
                    OnCellUpdate?.Invoke(null, new CellUpdateArgs<TCellType, TCell>(screenCoordinate, cell));
            }

            // Storage and chunking
            SetStateStorage(cell, storeState);
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
        /// <remarks>When setting multiple cells at once, use <see cref="SetCells(IEnumerable{TCell}, bool)"/> instead.</remarks>
        public void SetCell(int x, int y, TCellType cellType, bool storeState = false) 
            => SetCell(Convert(x, y, cellType), storeState);

        /// <summary>
        /// Update all <paramref name="cells"/> within the grid, this method is optimized for setting cells in unloaded chunks.
        /// </summary>
        /// <param name="cells">Collection of <typeparamref name="TCell"/></param>
        /// <param name="storeCellState">If <see langword="true"/>, stores all properties and field values of all <paramref name="cells"/>.</param>
        /// <remarks>If you want to control which cells to store state for, use the <see cref="SetCells(IEnumerable{TCell}, Func{TCell, bool}?)"/> overload.</remarks>
        public void SetCells(IEnumerable<TCell> cells, bool storeCellState) 
            => SetCells(cells, (s) => storeCellState);

        /// <summary>
        /// Update all <paramref name="cells"/> within the grid, this method is optimized for setting cells in unloaded chunks.
        /// </summary>
        /// <param name="cells">Collection of <typeparamref name="TCell"/></param>
        /// <param name="storeCellStateFunc">Method to decide which cell to store state for or not, default false if null.</param>
        public void SetCells(IEnumerable<TCell> cells, Func<TCell, bool>? storeCellStateFunc = null)
        {
            if (_chunkLoader == null)
            {
                // Handle non chunkloaded grid
                foreach (var cell in cells)
                {
                    if (!InBounds(cell)) continue;
                    SetCell(cell, storeCellStateFunc?.Invoke(cell) ?? false);
                }
            }
            else
            {
                // Handle chunkloaded grid
                _chunkLoader.SetChunkCells(cells, storeCellStateFunc, OnCellUpdate, IsWorldCoordinateOnScreen, ScreenCells);
            }
        }

        /// <summary>
        /// Retrieve the <typeparamref name="TCell"/> at position (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <remarks>When setting multiple cells at once, use <see cref="GetCells(IEnumerable{ValueTuple{int, int}})"/> instead.</remarks>
        /// <returns><typeparamref name="TCell"/>?</returns>
        public TCell? GetCell(int x, int y)
        {
            if (_chunkLoader == null && !InBounds(x, y)) return default;
            if (_chunkLoader == null && _storage != null && _storage.TryGetValue((x, y), out TCell? cell))
                return cell;
            if (_chunkLoader != null)
            {
                return _chunkLoader.GetChunkCell(x, y, true, IsWorldCoordinateOnScreen, ScreenCells);
            }
            return Convert(x, y, GetCellType(x, y));
        }

        /// <summary>
        /// Retrieves multiple <typeparamref name="TCell"/>
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public IReadOnlyList<TCell> GetCells(IEnumerable<(int, int)> positions)
        {
            if (_chunkLoader == null)
            {
                // Handle non chunkloaded grid
                var cells = new List<TCell>();
                foreach (var pos in positions)
                {
                    if (!InBounds(pos.Item1, pos.Item2)) continue;
                    var cell = GetCell(pos.Item1, pos.Item2);
                    if (cell != null)
                        cells.Add(cell);
                }
                return cells;
            }
            else
            {
                // Handle chunkloaded grid
                return _chunkLoader.GetChunkCells(positions, IsWorldCoordinateOnScreen, ScreenCells);
            }
        }

        /// <summary>
        /// Retrieve the <typeparamref name="TCellType"/> at position (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <returns><typeparamref name="TCellType"/></returns>
        public TCellType GetCellType(int x, int y)
        {
            if (_chunkLoader == null && !InBounds(x, y)) return default;
            if (_chunkLoader == null && _storage != null && _storage.TryGetValue((x, y), out TCell? cell))
                return cell.CellType;
            else if (_chunkLoader != null)
            {
                cell = _chunkLoader.GetChunkCell(x, y, true);
                return cell.CellType;
            }
            return ScreenCells[y * Width + x];
        }

        /// <summary>
        /// Validation to see if the coordinate (<paramref name="x"/>, <paramref name="y"/>) is within the <see cref="ScreenCells"/> bounds
        /// </summary>
        /// <remarks>Always true when chunking is used.</remarks>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <returns><see langword="true"/> or <see langword="false"/></returns>
        public bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        /// <summary>
        /// Validation to see if the (<typeparamref name="TCell"/>.X, <typeparamref name="TCell"/>.Y) is within the <see cref="ScreenCells"/> bounds
        /// </summary>
        /// <param name="cell"></param>
        /// <remarks>Always true when chunking is used.</remarks>
        /// <returns><see langword="true"/> or <see langword="false"/></returns>
        public bool InBounds(TCell cell)
        {
            return InBounds(cell.X, cell.Y);
        }

        private void SetStateStorage(TCell cell, bool storeState)
        {
            var coordinate = (cell.X, cell.Y);
            if (storeState && _chunkLoader == null)
            {
                if (_storage == null)
                    _storage = new Dictionary<(int x, int y), TCell>(new TupleComparer<int>());
                _storage[coordinate] = cell;
            }
            else if (!storeState && _chunkLoader == null && _storage != null)
            {
                _storage.Remove(coordinate);
                if (_storage.Count == 0)
                    _storage = null;
            }
            else if (_chunkLoader != null)
                _chunkLoader.SetChunkCell(cell, storeState, OnCellUpdate, IsWorldCoordinateOnScreen, ScreenCells);
        }

        /// <summary>
        /// Returns <see langword="true"/> if the world coordinate is within the viewport.
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <returns><see langword="true"/> or <see langword="false"/></returns>
        public bool IsWorldCoordinateOnViewPort(int x, int y)
        {
            return IsWorldCoordinateOnScreen(x, y, out _, out _);
        }

        internal bool IsWorldCoordinateOnScreen(int x, int y, out (int x, int y)? screenCoordinate, out int screenWidth)
        {
            screenWidth = Width;

            int minX = _centerCoordinate.x - Width / 2;
            int minY = _centerCoordinate.y - Height / 2;
            int maxX = minX + Width - 1;
            int maxY = minY + Height - 1;

            if (x >= minX && y >= minY && x <= maxX && y <= maxY)
            {
                screenCoordinate = WorldToScreenCoordinate(x, y);
                return true;
            }
            screenCoordinate = null;
            return false;
        }
    }
}
