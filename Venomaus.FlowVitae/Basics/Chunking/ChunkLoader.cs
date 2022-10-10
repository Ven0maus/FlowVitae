using System;
using System.Collections.Generic;
using System.Linq;
using Venomaus.FlowVitae.Basics.Procedural;
using Venomaus.FlowVitae.Helpers;

namespace Venomaus.FlowVitae.Basics.Chunking
{
    internal class ChunkLoader<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        private readonly int _seed;
        private readonly int _width, _height;
        private readonly IProceduralGen<TCellType, TCell> _generator;
        private readonly Func<int, int, TCellType, TCell> _cellTypeConverter;
        private readonly Dictionary<(int x, int y), TCellType[]> _chunks;
        private readonly HashSet<(int x, int y)> _tempLoadedChunks;
        private Dictionary<(int x, int y), Dictionary<(int x, int y), TCell>>? _modifiedCellsInChunks;

        public (int x, int y) CurrentChunk { get; private set; }
        public bool RaiseOnlyOnCellTypeChange { get; set; } = true;

        public ChunkLoader(int width, int height, IProceduralGen<TCellType, TCell> generator, Func<int, int, TCellType, TCell> cellTypeConverter)
        {
            _width = width;
            _height = height;
            _seed = generator.Seed;
            _generator = generator;
            _cellTypeConverter = cellTypeConverter;
            _tempLoadedChunks = new HashSet<(int x, int y)>(new TupleComparer<int>());
            _chunks = new Dictionary<(int x, int y), TCellType[]>(new TupleComparer<int>());
        }

        public void ClearGridCache()
        {
            _modifiedCellsInChunks = null;
        }

        /// <summary>
        /// Remaps the cell coordinate to the Grid's coordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public (int x, int y) RemapChunkCoordinate(int x, int y)
        {
            _ = GetChunk(x, y, out var chunkCoordinate);
            return RemapChunkCoordinate(x, y, chunkCoordinate);
        }

        private static (int x, int y) RemapChunkCoordinate(int x, int y, (int x, int y) chunkCoordinate)
        {
            return (x: Math.Abs(x - chunkCoordinate.x), y: Math.Abs(y - chunkCoordinate.y));
        }

        public void SetCurrentChunk(int x, int y)
        {
            CurrentChunk = GetChunkCoordinate(x, y);

            // Load new chunks
            LoadChunksAround(x, y, true);

            // Unload any other chunks
            UnloadNonMandatoryChunks();
        }

        public (int x, int y)[] GetLoadedChunks()
        {
            return _chunks.Keys.ToArray();
        }

        public void LoadChunksAround(int x, int y, bool includeSourceChunk)
        {
            var chunkCoordinate = GetChunkCoordinate(x, y);

            if (includeSourceChunk)
                LoadChunk(chunkCoordinate.x, chunkCoordinate.y, out _);

            var neighbors = GetNeighborChunks(chunkCoordinate.x, chunkCoordinate.y);
            foreach (var neighbor in neighbors)
                LoadChunk(neighbor.x, neighbor.y, out _);
        }

        public void UnloadNonMandatoryChunks()
        {
            var mandatoryChunks = GetMandatoryChunks();
            var allChunks = _chunks.Select(a => a.Key).ToArray();
            foreach (var chunk in allChunks)
            {
                if (!mandatoryChunks.Contains(chunk))
                    UnloadChunk(chunk.x, chunk.y);
            }
        }

        public HashSet<(int x, int y)> GetMandatoryChunks()
        {
            return new HashSet<(int x, int y)>(GetNeighborChunks(), new TupleComparer<int>())
            {
                CurrentChunk
            };
        }

        public TCell GetChunkCell(int x, int y, bool loadChunk = false, Checker? isWorldCoordinateOnScreen = null, TCellType[]? screenCells = null)
        {
            var chunkCoordinate = GetChunkCoordinate(x, y);
            var remappedCoordinate = RemapChunkCoordinate(x, y, chunkCoordinate);

            // Check if there are modified cell tiles within this chunk
            if (_modifiedCellsInChunks != null && _modifiedCellsInChunks
                .TryGetValue(chunkCoordinate, out var cells) &&
                cells.TryGetValue(remappedCoordinate, out var cell))
            {
                // Return the modified cell if it exists
                return cell;
            }

            // Check if coordinate is within viewport
            var chunk = GetChunk(x, y, out _);
            if (isWorldCoordinateOnScreen != null && screenCells != null &&
                isWorldCoordinateOnScreen.Invoke(x, y, out (int x, int y)? screenCoordinate, out var screenWidth) &&
                screenCoordinate != null)
            {
                if (chunk != null)
                {
                    // Adjust screen cell if it doesn't match with chunk cell && no modified cell was stored
                    var screenCell = screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x];
                    var chunkCell = chunk[remappedCoordinate.y * _width + remappedCoordinate.x];
                    if (!screenCell.Equals(chunkCell))
                    {
                        screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x] = chunkCell;
                    }
                }

                return _cellTypeConverter(x, y, screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x]);
            }

            bool wasChunkLoaded = false;
            if (loadChunk)
                wasChunkLoaded = LoadChunk(x, y, out _);

            // Load chunk after all other options are validated
            chunk = GetChunk(x, y, out _);
            if (chunk == null)
                throw new Exception("Something went wrong during chunk retrieval.");

            if (loadChunk && wasChunkLoaded)
                UnloadChunk(x, y);
            // Return the non-modified cell
            return _cellTypeConverter(x, y,
                chunk[remappedCoordinate.y * _width + remappedCoordinate.x]);
        }

        public IEnumerable<TCell> GetChunkCells(IEnumerable<(int x, int y)> positions, Checker? isWorldCoordinateOnScreen = null, TCellType[]? screenCells = null)
        {
            var cells = new List<TCell>();
            foreach (var (x, y) in positions)
            {
                if (LoadChunk(x, y, out var chunkCoordinate))
                    _tempLoadedChunks.Add(chunkCoordinate);
                var cell = GetChunkCell(x, y, false, isWorldCoordinateOnScreen, screenCells);
                yield return cell;
            }
            foreach (var (x, y) in _tempLoadedChunks)
                UnloadChunk(x, y);
            _tempLoadedChunks.Clear();
        }

        public void SetChunkCell(TCell cell, bool storeState = false, 
            EventHandler<CellUpdateArgs<TCellType, TCell>>? onCellUpdate = null, 
            Checker? isWorldCoordinateOnScreen = null, 
            TCellType[]? screenCells = null)
        {
            var chunkCoordinate = GetChunkCoordinate(cell.X, cell.Y);
            var remappedCoordinate = RemapChunkCoordinate(cell.X, cell.Y, chunkCoordinate);

            if (!storeState && _modifiedCellsInChunks != null && _modifiedCellsInChunks.TryGetValue(chunkCoordinate, out var storedCells))
            {
                storedCells.Remove(remappedCoordinate);
                if (storedCells.Count == 0)
                    _modifiedCellsInChunks.Remove(chunkCoordinate);
                if (_modifiedCellsInChunks.Count == 0)
                    _modifiedCellsInChunks = null;
            }
            else if (storeState)
            {
                // Check if there are modified cell tiles within this chunk
                if (_modifiedCellsInChunks == null)
                    _modifiedCellsInChunks = new Dictionary<(int x, int y), Dictionary<(int x, int y), TCell>>(new TupleComparer<int>());
                if (!_modifiedCellsInChunks.TryGetValue(chunkCoordinate, out storedCells))
                {
                    storedCells = new Dictionary<(int x, int y), TCell>(new TupleComparer<int>());
                    _modifiedCellsInChunks.Add(chunkCoordinate, storedCells);
                }
                storedCells[remappedCoordinate] = cell;
            }

            if (isWorldCoordinateOnScreen != null && screenCells != null &&
                isWorldCoordinateOnScreen(cell.X, cell.Y, out var screenCoordinate, out var screenWidth)
                && screenCoordinate != null)
            {
                var prev = screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x];
                screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x] = cell.CellType;

                if (!RaiseOnlyOnCellTypeChange || !prev.Equals(cell.CellType))
                    onCellUpdate?.Invoke(null, new CellUpdateArgs<TCellType, TCell>(screenCoordinate.Value, cell));
                
                var chunk = GetChunk(cell.X, cell.Y, out _);
                if (chunk != null)
                {
                    chunk[remappedCoordinate.y * _width + remappedCoordinate.x] = cell.CellType;
                }
            }
        }

        public delegate bool Checker(int x, int y, out (int x, int y)? coordinate, out int screenWidth);
        public void SetChunkCells(IEnumerable<TCell> cells, Func<TCell, bool>? storeCellStateFunc = null, EventHandler<CellUpdateArgs<TCellType, TCell>>? onCellUpdate = null, Checker? isWorldCoordinateOnScreen = null, TCellType[]? screenCells = null)
        {
            foreach (var cell in cells)
                SetChunkCell(cell, storeCellStateFunc?.Invoke(cell) ?? false, onCellUpdate: onCellUpdate, isWorldCoordinateOnScreen: isWorldCoordinateOnScreen, screenCells: screenCells);
        }

        public (int x, int y) GetNeighborChunk(int x, int y, Direction direction)
        {
            var chunkCoordinate = GetChunkCoordinate(x, y);
            int chunkX = chunkCoordinate.x;
            int chunkY = chunkCoordinate.y;
            switch (direction)
            {
                case Direction.North:
                    chunkY = CurrentChunk.y + _height;
                    break;
                case Direction.East:
                    chunkX = CurrentChunk.x + _width;
                    break;
                case Direction.South:
                    chunkY = CurrentChunk.y - _height;
                    break;
                case Direction.West:
                    chunkX = CurrentChunk.x - _width;
                    break;
                case Direction.NorthEast:
                    chunkY = CurrentChunk.y + _height;
                    chunkX = CurrentChunk.x + _width;
                    break;
                case Direction.NorthWest:
                    chunkY = CurrentChunk.y + _height;
                    chunkX = CurrentChunk.x - _width;
                    break;
                case Direction.SouthEast:
                    chunkY = CurrentChunk.y - _height;
                    chunkX = CurrentChunk.x + _width;
                    break;
                case Direction.SouthWest:
                    chunkY = CurrentChunk.y - _height;
                    chunkX = CurrentChunk.x - _width;
                    break;
            }
            return GetChunkCoordinate(chunkX, chunkY);
        }

        public (int x, int y)[] GetNeighborChunks(int x, int y)
        {
            var chunks = new[]
            {
                GetNeighborChunk(x, y, Direction.North),
                GetNeighborChunk(x, y, Direction.East),
                GetNeighborChunk(x, y, Direction.South),
                GetNeighborChunk(x, y, Direction.West),
                GetNeighborChunk(x, y, Direction.NorthEast),
                GetNeighborChunk(x, y, Direction.NorthWest),
                GetNeighborChunk(x, y, Direction.SouthEast),
                GetNeighborChunk(x, y, Direction.SouthWest)
            };
            return chunks;
        }

        public (int x, int y)[] GetNeighborChunks()
        {
            return GetNeighborChunks(CurrentChunk.x, CurrentChunk.y);
        }

        public bool LoadChunk(int x, int y, out (int x, int y) chunkCoordinate)
        {
            chunkCoordinate = GetChunkCoordinate(x, y);

            if (!_chunks.ContainsKey(chunkCoordinate))
            {
                GenerateChunk(chunkCoordinate);
                return true;
            }
            return false;
        }

        public bool UnloadChunk(int x, int y, bool forceUnload = false)
        {
            var coordinate = GetChunkCoordinate(x, y);

            // Check that we are not unloading current or neighbor chunks
            if (!forceUnload)
            {
                if (CurrentChunk == coordinate) return false;
                var neighborChunks = GetNeighborChunks();
                if (neighborChunks.Any(m => m.x == x && m.y == y)) return false;
            }

            if (_chunks.ContainsKey(coordinate))
            {
                _chunks.Remove(coordinate);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the base coordinate of the cell's chunk
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public (int x, int y) GetChunkCoordinate(int x, int y)
        {
            return GetCoordinateBySizeNoConversion(x, y, _width, _height);
        }

        public void CenterViewPort(int x, int y, int viewPortWidth, int viewPortHeight, 
            (int x, int y) centerCoordinate, Checker isWorldCoordinateOnScreen, TCellType[] screenCells,
            EventHandler<CellUpdateArgs<TCellType, TCell>>? onCellUpdate)
        {
            var minX = x - (viewPortWidth / 2);
            var minY = y - (viewPortHeight / 2);

            // Update the current chunk
            var centerChunk = GetChunkCoordinate(x, y);
            SetCurrentChunk(centerChunk.x, centerChunk.y);

            for (var xX = 0; xX < viewPortWidth; xX++)
            {
                for (var yY = 0; yY < viewPortHeight; yY++)
                {
                    var cellX = minX + xX;
                    var cellY = minY + yY;

                    if (LoadChunk(cellX, cellY, out var chunkCoordinate))
                        _tempLoadedChunks.Add(chunkCoordinate);

                    var cell = GetChunkCell(cellX, cellY, false, isWorldCoordinateOnScreen, screenCells);
                    var screenCoordinate = WorldToScreenCoordinate(cell.X, cell.Y, viewPortWidth, viewPortHeight, centerCoordinate);
                    screenCells[screenCoordinate.y * viewPortWidth + screenCoordinate.x] = cell.CellType;
                    onCellUpdate?.Invoke(null, new CellUpdateArgs<TCellType, TCell>(screenCoordinate, cell));
                }
            }

            foreach (var chunk in _tempLoadedChunks)
                UnloadChunk(chunk.x, chunk.y);
            _tempLoadedChunks.Clear();
        }

        public static (int x, int y) WorldToScreenCoordinate(int x, int y, int viewPortWidth, int viewPortHeight, (int x, int y) centerCoordinate)
        {
            var halfCenterX = centerCoordinate.x - (viewPortWidth / 2);
            var halfCenterY = centerCoordinate.y - (viewPortHeight / 2);
            var modifiedPos = (x: x - halfCenterX, y: y - halfCenterY);
            return modifiedPos;
        }

        private TCellType[]? GetChunk(int x, int y, out (int x, int y) chunkCoordinate)
        {
            chunkCoordinate = GetChunkCoordinate(x, y);
            return _chunks.TryGetValue(chunkCoordinate, out var chunk) ? chunk : null;
        }

        private TCellType[] GenerateChunk((int x, int y) coordinate)
        {
            // Get a unique hash seed based on the chunk (x,y) and the main seed
            var chunkSeed = Fnv1a.Hash32(coordinate.x, coordinate.y, _seed);
            // Generate chunk data
            var chunk = _generator.Generate(chunkSeed, _width, _height);
            _chunks.Add(coordinate, chunk);
            return chunk;
        }

        private static (int x, int y) GetCoordinateBySizeNoConversion(int x, int y, int width, int height)
        {
            if (x < 0 && x % width != 0) x -= width;
            if (y < 0 && y % height != 0) y -= height;
            var chunkX = width * (x / width);
            var chunkY = height * (y / height);
            return (chunkX, chunkY);
        }
    }
}
