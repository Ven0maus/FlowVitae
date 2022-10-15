using System;
using System.Collections.Generic;
using System.Linq;
using Venomaus.FlowVitae.Basics.Chunking;
using Venomaus.FlowVitae.Basics.Procedural;
using Venomaus.FlowVitae.Helpers;

namespace Venomaus.FlowVitae.Basics
{
    /// <summary>
    /// Base class which provides basic grid functionality
    /// </summary>
    /// <typeparam name="TCellType">The cell type to be used within the <see cref="GridBase{TCellType, TCell, TChunkData}"/></typeparam>
    /// <typeparam name="TCell">The wrapper object used to wrap around the cell type</typeparam>
    /// <typeparam name="TChunkData">The custom chunk data type to be returned from chunks</typeparam>
    public abstract class GridBase<TCellType, TCell, TChunkData>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
        where TChunkData : class, IChunkData
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
        internal readonly ChunkLoader<TCellType, TCell, TChunkData>? _chunkLoader;

        /// <summary>
        /// True if chunks should be loaded in a seperate thread if possible, this improves performance.
        /// This is by default enabled.
        /// </summary>
        /// <remarks>This property works only for chunked grids.</remarks>
        public bool UseThreading
        {
            get
            {
                return _chunkLoader != null && _chunkLoader.UseThreading;
            }
            set
            {
                if (_chunkLoader != null)
                    _chunkLoader.UseThreading = value;
            }
        }

        /// <summary>
        /// Raised every time a screen cell is updated, can be used for rendering updates.
        /// </summary>
        /// <remarks>See <see cref="RaiseOnlyOnCellTypeChange"/> to control how this event is raised.</remarks>
        public event EventHandler<CellUpdateArgs<TCellType, TCell>>? OnCellUpdate;

        /// <summary>
        /// Raised every time one of the main chunks gets loaded.
        /// (There are always 9 chunks loaded: The center chunk and its 8 neighbor chunks.)
        /// </summary>
        public event EventHandler<ChunkUpdateArgs>? OnChunkLoad;
        /// <summary>
        /// Raised every time one of the main chunks gets unloaded.
        /// (There are always 9 chunks loaded: The center chunk and its 8 neighbor chunks.)
        /// </summary>
        public event EventHandler<ChunkUpdateArgs>? OnChunkUnload;

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

        private bool _viewPortInitialized = false;

        /// <summary>
        /// Constructor for <see cref="GridBase{TCellType, TCell}"/>
        /// </summary>
        /// <remarks>Initializes a grid that does not use chunking.</remarks>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public GridBase(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new Exception("Cannot define a grid with a viewport width/height smaller or equal to 0");

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
        public GridBase(int viewPortWidth, int viewPortHeight, int chunkWidth, int chunkHeight, IProceduralGen<TCellType, TCell, TChunkData>? generator)
            : this(viewPortWidth, viewPortHeight)
        {
            if (generator == null) return;
            if (chunkWidth <= 0 || chunkHeight <= 0)
                throw new Exception("Cannot define a grid with a chunk width/height smaller or equal to 0");

            // Disable for initial load
            UseThreading = false;

            // Initialize chunkloader if grid uses chunks
            _chunkLoader = new ChunkLoader<TCellType, TCell, TChunkData>(viewPortWidth, viewPortHeight, chunkWidth, chunkHeight, generator, Convert);
            _chunkLoader.LoadChunksAround(0, 0, true);

            // Default center on the middle of the viewport
            Center(viewPortWidth / 2, viewPortHeight / 2);
            _viewPortInitialized = true;
            UseThreading = true;
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
        public GridBase(int viewPortWidth, int viewPortHeight, IProceduralGen<TCellType, TCell, TChunkData>? generator)
            : this(viewPortWidth, viewPortHeight, viewPortWidth, viewPortHeight, generator)
        { }

        /// <summary>
        /// Sets all cells of which the state was stored, to be eligible for garbage collection.
        /// </summary>
        /// <remarks>Use this when you want to remove all stored states from the grid, Note: this also resets the viewport cells for chunkloaded grids.</remarks>
        public void ClearCache()
        {
            _storage = null;
            _chunkLoader?.ClearGridCache();

            // Reload all chunks
            if (_chunkLoader != null)
            {
                // Clear chunks and data
                foreach (var (chunkX, chunkY) in _chunkLoader.GetLoadedChunks())
                    _chunkLoader.UnloadChunk(chunkX, chunkY, true);

                // Re-center and load chunks within the main thread
                _viewPortInitialized = false;
                UseThreading = false;
                _chunkLoader.LoadChunksAround(_centerCoordinate.x, _centerCoordinate.y, true);
                Center(_centerCoordinate.x, _centerCoordinate.y);
                _viewPortInitialized = true;
                UseThreading = true;
            }
        }

        /// <summary>
        /// Retrieves the chunk data for this chunk. Returns null if no data was specified during generation.
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <returns><typeparamref name="TChunkData"/> / null</returns>
        public TChunkData? GetChunkData(int x, int y)
        {
            return _chunkLoader?.GetChunkData(x, y);
        }

        /// <summary>
        /// Allows to store the chunk data in the internal cache, this will cause the chunk data not to be reloaded on chunk load.
        /// </summary>
        public void StoreChunkData(TChunkData chunkData)
        {
            _chunkLoader?.StoreChunkData(chunkData);
        }

        /// <summary>
        /// Removes the chunk data from the internal cache, this will cause the chunk data to be reloaded on chunk load.
        /// </summary>
        /// <remarks>The chunk data will only be refreshed on reload of the chunk.</remarks>
        public void RemoveChunkData(TChunkData chunkData, bool reloadChunk = false)
        {
            _chunkLoader?.RemoveChunkData(chunkData, reloadChunk);
        }

        /// <summary>
        /// Overwrites the Convert method with a custom implementation without having to create a new <see cref="GridBase{TCellType, TCell}"/> implementation.
        /// </summary>
        /// <param name="converter">Converter func that resembles the Convert method</param>
        public virtual void SetCustomConverter(Func<int, int, TCellType, TCell>? converter)
        {
            _customConverter = converter;
        }

        /// <summary>
        /// Retrieves the unique chunk seed, for the chunk where the cell (x,y) is situated in.
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <returns>Unique chunk seed</returns>
        public int GetChunkSeed(int x, int y)
        {
            if (_chunkLoader == null) return 0;
            return _chunkLoader.GetChunkSeed(x, y);
        }

        /// <summary>
        /// Centers the grid on the specified coordinate
        /// </summary>
        /// <remarks>Can only be used for grids that use chunking.</remarks>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        public virtual void Center(int x, int y)
        {
            if (_chunkLoader == null) return;

            var prevCenterCoordinate = _centerCoordinate;
            _centerCoordinate = (x, y);

            _chunkLoader.CenterViewPort(x, y, Width, Height, prevCenterCoordinate, _centerCoordinate,
                IsWorldCoordinateOnScreen, ScreenCells, OnCellUpdate, _viewPortInitialized, OnChunkLoad, OnChunkUnload);
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
        public (int x, int y) ScreenToWorldCoordinate(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
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
        public (int x, int y) WorldToScreenCoordinate(int x, int y)
        {
            if (_chunkLoader == null)
            {
                if (!InBounds(x, y))
                    throw new Exception("Invalid world coordinate, must be within screen bounds (Width * Height).");
                return (x, y);
            }
            return ChunkLoader<TCellType, TCell, TChunkData>.WorldToScreenCoordinate(x, y, Width, Height, _centerCoordinate);
        }

        /// <summary>
        /// Gets an enumerable of all the world coordinates that reside within the viewport
        /// </summary>
        /// <param name="criteria">Can provide custom criteria on which celltypes you want from the viewport</param>
        /// <remarks>Cell (x, y) are adjusted to match the viewport (x, y).</remarks>
        /// <returns></returns>
        public IEnumerable<(int x, int y)> GetViewPortWorldCoordinates(Func<TCellType, bool>? criteria = null)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (criteria != null && !criteria.Invoke(ScreenCells[y * Width + x])) continue;
                    yield return ScreenToWorldCoordinate(x, y);
                }
            }
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
        public virtual void SetCell(TCell cell, bool storeState = false)
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
        public virtual void SetCell(int x, int y, TCellType cellType, bool storeState = false)
            => SetCell(Convert(x, y, cellType), storeState);

        /// <summary>
        /// Update all <paramref name="cells"/> within the grid, this method is optimized for setting cells in unloaded chunks.
        /// </summary>
        /// <param name="cells">Collection of <typeparamref name="TCell"/></param>
        /// <param name="storeCellState">If <see langword="true"/>, stores all properties and field values of all <paramref name="cells"/>.</param>
        /// <remarks>If you want to control which cells to store state for, use the <see cref="SetCells(IEnumerable{TCell}, Func{TCell, bool}?)"/> overload.</remarks>
        public virtual void SetCells(IEnumerable<TCell> cells, bool storeCellState)
            => SetCells(cells, (s) => storeCellState);

        /// <summary>
        /// Update all <paramref name="cells"/> within the grid, this method is optimized for setting cells in unloaded chunks.
        /// </summary>
        /// <param name="cells">Collection of <typeparamref name="TCell"/></param>
        /// <param name="storeCellStateFunc">Method to decide which cell to store state for or not, default false if null.</param>
        public virtual void SetCells(IEnumerable<TCell> cells, Func<TCell, bool>? storeCellStateFunc = null)
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
        /// Returns the neighbor cells based on the specified adjacency rule
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <param name="adjacencyRule">Specifies the adjacent directions to retrieve</param>
        /// <returns></returns>
        public virtual IEnumerable<TCell> GetNeighbors(int x, int y, AdjacencyRule adjacencyRule)
        {
            var positions = new List<(int x, int y)>();
            switch (adjacencyRule)
            {
                case AdjacencyRule.FourWay:
                    for (int xX = x - 1; xX <= x + 1; xX++)
                    {
                        if (xX == x) continue;
                        if (_chunkLoader == null && !InBounds(xX, y)) continue;
                        positions.Add((xX, y));
                    }
                    for (int yY = y - 1; yY <= y + 1; yY++)
                    {
                        if (yY == y) continue;
                        if (_chunkLoader == null && !InBounds(x, yY)) continue;
                        positions.Add((x, yY));
                    }
                    break;
                case AdjacencyRule.EightWay:
                    for (int xX = x - 1; xX <= x + 1; xX++)
                    {
                        for (int yY = y - 1; yY <= y + 1; yY++)
                        {
                            if (xX == x && yY == y) continue;
                            if (_chunkLoader == null && !InBounds(xX, yY)) continue;
                            positions.Add((xX, yY));
                        }
                    }
                    break;
            }
            return GetCells(positions);
        }

        /// <summary>
        /// Retrieve the <typeparamref name="TCell"/> at position (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <remarks>When setting multiple cells at once, use <see cref="GetCells(IEnumerable{ValueTuple{int, int}})"/> instead.</remarks>
        /// <returns><typeparamref name="TCell"/>?</returns>
        public virtual TCell? GetCell(int x, int y)
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
        public virtual IEnumerable<TCell> GetCells(IEnumerable<(int, int)> positions)
        {
            if (_chunkLoader == null)
            {
                // Handle non chunkloaded grid
                foreach (var pos in positions)
                {
                    if (!InBounds(pos.Item1, pos.Item2)) continue;
                    var cell = GetCell(pos.Item1, pos.Item2);
                    if (cell != null)
                        yield return cell;
                }
            }
            else
            {
                // Handle chunkloaded grid
                foreach (var cell in _chunkLoader.GetChunkCells(positions, IsWorldCoordinateOnScreen, ScreenCells))
                    yield return cell;
            }
        }

        /// <summary>
        /// Retrieve the <typeparamref name="TCellType"/> at position (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <returns><typeparamref name="TCellType"/></returns>
        public virtual TCellType GetCellType(int x, int y)
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
            if (_chunkLoader != null) return true;
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

    /// <summary>
    /// Base class which provides basic grid functionality
    /// </summary>
    /// <typeparam name="TCellType">The cell type to be used within the <see cref="GridBase{TCellType, TCell}"/></typeparam>
    /// <typeparam name="TCell">The wrapper object used to wrap around the cell type</typeparam>
    public abstract class GridBase<TCellType, TCell> : GridBase<TCellType, TCell, IChunkData>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        /// <inheritdoc />
        protected GridBase(int width, int height) : base(width, height)
        { }

        /// <inheritdoc />
        protected GridBase(int viewPortWidth, int viewPortHeight, IProceduralGen<TCellType, TCell>? generator) : base(viewPortWidth, viewPortHeight, generator)
        { }

        /// <inheritdoc />
        protected GridBase(int viewPortWidth, int viewPortHeight, int chunkWidth, int chunkHeight, IProceduralGen<TCellType, TCell>? generator) : base(viewPortWidth, viewPortHeight, chunkWidth, chunkHeight, generator)
        { }
    }
}
