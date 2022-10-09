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
        private Dictionary<(int x, int y), Dictionary<(int x, int y), TCell>>? _modifiedCellsInChunks;

        public (int x, int y) CurrentChunk { get; private set; }

        public ChunkLoader(int width, int height, IProceduralGen<TCellType, TCell> generator, Func<int, int, TCellType, TCell> cellTypeConverter)
        {
            _width = width;
            _height = height;
            _seed = generator.Seed;
            _generator = generator;
            _cellTypeConverter = cellTypeConverter;
            _chunks = new Dictionary<(int x, int y), TCellType[]>(new TupleComparer<int>());
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
            if (_chunks == null) return Array.Empty<(int x, int y)>();
            return _chunks.Keys.ToArray();
        }

        public void LoadChunksAround(int x, int y, bool includeSourceChunk)
        {
            var chunkCoordinate = GetChunkCoordinate(x, y);

            if (includeSourceChunk)
                LoadChunk(chunkCoordinate.x, chunkCoordinate.y);

            var neighbors = GetNeighborChunks(chunkCoordinate.x, chunkCoordinate.y);
            foreach (var neighbor in neighbors)
                LoadChunk(neighbor.x, neighbor.y);
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

        public TCell? GetChunkCell(int x, int y, bool loadChunk = false)
        {
            bool wasChunkLoaded = false;
            if (loadChunk)
                wasChunkLoaded = LoadChunk(x, y);

            var chunk = GetChunk(x, y, out var chunkCoordinate);
            if (chunk != null)
            {
                var remappedCoordinate = RemapChunkCoordinate(x, y, chunkCoordinate);

                // Check if there are modified cell tiles within this chunk
                if (_modifiedCellsInChunks != null && _modifiedCellsInChunks
                    .TryGetValue(chunkCoordinate, out var cells) &&
                    cells.TryGetValue(remappedCoordinate, out var cell))
                {
                    if (loadChunk && wasChunkLoaded)
                        UnloadChunk(x, y);
                    // Return the modified cell if it exists
                    return cell;
                }

                if (loadChunk && wasChunkLoaded)
                    UnloadChunk(x, y);
                // Return the non-modified cell
                return _cellTypeConverter(x, y,
                    chunk[remappedCoordinate.y * _width + remappedCoordinate.x]);
            }
            return null;
        }

        public IReadOnlyList<TCell> GetChunkCells(IEnumerable<(int x, int y)> positions)
        {
            var loadedChunks = new List<(int x, int y)>();
            var cells = new List<TCell>();
            foreach (var (x, y) in positions)
            {
                if (LoadChunk(x, y))
                    loadedChunks.Add((x, y));
                var cell = GetChunkCell(x, y);
                if (cell != null)
                    cells.Add(cell);
            }
            foreach (var (x, y) in loadedChunks)
                UnloadChunk(x, y);
            return cells;
        }

        public void SetChunkCell(TCell cell, bool storeState = false, bool loadChunk = false, EventHandler<CellUpdateArgs<TCellType, TCell>>? onCellUpdate = null, Checker? isWorldCoordinateOnScreen = null, TCellType[]? screenCells = null)
        {
            bool wasChunkLoaded = false;
            if (loadChunk)
                wasChunkLoaded = LoadChunk(cell.X, cell.Y);

            var chunk = GetChunk(cell.X, cell.Y, out var chunkCoordinate);
            if (chunk != null)
            {
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

                var prev = chunk[remappedCoordinate.y * _width + remappedCoordinate.x];

                // Adjust chunk cell & stored cell
                chunk[remappedCoordinate.y * _width + remappedCoordinate.x] = cell.CellType;

                if (isWorldCoordinateOnScreen != null && screenCells != null &&
                    isWorldCoordinateOnScreen(cell.X, cell.Y, out (int x, int y)? screenCoordinate, out int screenWidth) &&
                    screenCoordinate != null)
                {
                    screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x] = cell.CellType;

                    if (!storeState && !prev.Equals(cell.CellType))
                        onCellUpdate?.Invoke(null, new CellUpdateArgs<TCellType, TCell>(screenCoordinate.Value, cell));
                    else if (storeState)
                        onCellUpdate?.Invoke(null, new CellUpdateArgs<TCellType, TCell>(screenCoordinate.Value, cell));
                }
            }

            if (loadChunk && wasChunkLoaded)
                UnloadChunk(cell.X, cell.Y);
        }

        public void SetChunkCells(IEnumerable<TCell> cells, bool storeCellState) 
            => SetChunkCells(cells, (s) => storeCellState);

        public delegate bool Checker(int x, int y, out (int x, int y)? coordinate, out int screenWidth);
        public void SetChunkCells(IEnumerable<TCell> cells, Func<TCell, bool>? storeCellStateFunc = null, EventHandler<CellUpdateArgs<TCellType, TCell>>? onCellUpdate = null, Checker? isWorldCoordinateOnScreen = null, TCellType[]? screenCells = null)
        {
            var loadedChunks = new List<(int x, int y)>();
            foreach (var cell in cells)
            {
                if (LoadChunk(cell.X, cell.Y))
                    loadedChunks.Add((cell.X, cell.Y));
                SetChunkCell(cell, storeCellStateFunc?.Invoke(cell) ?? false, onCellUpdate: onCellUpdate, isWorldCoordinateOnScreen: isWorldCoordinateOnScreen, screenCells: screenCells);
            }
            foreach (var (x, y) in loadedChunks)
                UnloadChunk(x, y);
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

        public bool LoadChunk(int x, int y)
        {
            var coordinate = GetChunkCoordinate(x, y);

            if (!_chunks.ContainsKey(coordinate))
            {
                GenerateChunk(coordinate);
                return true;
            }
            return false;
        }

        public void LoadChunk(TCell cell) => LoadChunk(cell.X, cell.Y);

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

        public void UnloadChunk(TCell cell) => UnloadChunk(cell.X, cell.Y);

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
            var chunk = _generator?.Generate(chunkSeed, _width, _height);
            if (chunk == null || chunk.Length != (_width * _height))
            {
                throw new Exception(chunk == null ?
                    "Chunk generator returned null chunk data." :
                    "Chunk generator returned invalid sized chunk data, must be of length (width * height).");
            }

            _chunks.Add(coordinate, chunk);

            return chunk;
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

        private (int x, int y) GetCoordinateBySizeConversion(int x, int y, int width, int height)
        {
            // TODO: Performance test
            var chunkX = (int)(width * Math.Floor(((double)x / width)));
            var chunkY = (int)(height * Math.Floor(((double)y / height)));
            return (chunkX, chunkY);
        }

        private (int x, int y) GetCoordinateBySizeNoConversion(int x, int y, int width, int height)
        {
            if (x < 0 && x % width != 0) x -= width;
            if (y < 0 && y % height != 0) y -= height;
            var chunkX = width * (x / width);
            var chunkY = height * (y / height);
            return (chunkX, chunkY);
        }
    }
}
