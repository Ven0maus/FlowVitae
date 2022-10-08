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
            _chunks = new Dictionary<(int x, int y), TCellType[]>();
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
                    _chunks.Remove(chunk);
            }
        }

        private HashSet<(int x, int y)> GetMandatoryChunks()
        {
            return new HashSet<(int x, int y)>(GetNeighborChunks())
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
                return _cellTypeConverter(remappedCoordinate.x, remappedCoordinate.y,
                    chunk[remappedCoordinate.y * _width + remappedCoordinate.x]);
            }
            return null;
        }

        public IEnumerable<TCell> GetChunkCells(IEnumerable<(int, int)> positions)
        {
            var loadedChunks = new List<(int x, int y)>();
            foreach (var pos in positions)
            {
                if (LoadChunk(pos.Item1, pos.Item2))
                    loadedChunks.Add((pos.Item1, pos.Item2));
                var cell = GetChunkCell(pos.Item1, pos.Item2);
                if (cell != null)
                    yield return cell;
            }
            foreach (var (x, y) in loadedChunks)
                UnloadChunk(x, y);
        }

        public void SetChunkCell(int x, int y, TCell cell, bool storeState = false, bool loadChunk = false, EventHandler<TCell>? onCellUpdate = null, Checker? isWorldCoordinateOnScreen = null, TCellType[]? screenCells = null)
        {
            bool wasChunkLoaded = false;
            if (loadChunk)
                wasChunkLoaded = LoadChunk(x, y);

            var chunk = GetChunk(x, y, out var chunkCoordinate);
            if (chunk != null)
            {
                var remappedCoordinate = RemapChunkCoordinate(x, y, chunkCoordinate);
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
                        _modifiedCellsInChunks = new Dictionary<(int x, int y), Dictionary<(int x, int y), TCell>>();
                    if (!_modifiedCellsInChunks.TryGetValue(chunkCoordinate, out storedCells))
                    {
                        storedCells = new Dictionary<(int x, int y), TCell>();
                        _modifiedCellsInChunks.Add(chunkCoordinate, storedCells);
                    }
                    storedCells[remappedCoordinate] = cell;
                }

                var prev = chunk[remappedCoordinate.y * _width + remappedCoordinate.x];

                // Adjust chunk cell & stored cell
                chunk[remappedCoordinate.y * _width + remappedCoordinate.x] = cell.CellType;

                if (isWorldCoordinateOnScreen != null && screenCells != null &&
                    isWorldCoordinateOnScreen(x, y, out (int x, int y)? screenCoordinate, out int screenWidth) &&
                    screenCoordinate != null)
                {
                    screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x] = cell.CellType;
                }

                if (!storeState && !prev.Equals(cell.CellType))
                    onCellUpdate?.Invoke(null, cell);
                else if (storeState)
                    onCellUpdate?.Invoke(null, cell);
            }

            if (loadChunk && wasChunkLoaded)
                UnloadChunk(x, y);
        }

        public void SetChunkCells(IEnumerable<TCell> cells, bool storeCellState) 
            => SetChunkCells(cells, (s) => storeCellState);

        public delegate bool Checker(int x, int y, out (int x, int y)? coordinate, out int screenWidth);
        public void SetChunkCells(IEnumerable<TCell> cells, Func<TCell, bool>? storeCellStateFunc = null, EventHandler<TCell>? onCellUpdate = null, Checker? isWorldCoordinateOnScreen = null, TCellType[]? screenCells = null)
        {
            var loadedChunks = new List<(int x, int y)>();
            foreach (var cell in cells)
            {
                if (LoadChunk(cell.X, cell.Y))
                    loadedChunks.Add((cell.X, cell.Y));
                SetChunkCell(cell.X, cell.Y, cell, storeCellStateFunc?.Invoke(cell) ?? false, onCellUpdate: onCellUpdate, isWorldCoordinateOnScreen: isWorldCoordinateOnScreen, screenCells: screenCells);
            }
            foreach (var (x, y) in loadedChunks)
                UnloadChunk(x, y);
        }

        private (int x, int y) GetNeighborChunk(Direction direction)
        {
            // TODO: Implement neighbor chunking
            return (0, 0);
        }

        private (int x, int y) GetNeighborChunk(int x, int y, Direction direction)
        {
            // TODO: Implement neighbor chunking
            return (0, 0);
        }

        private (int x, int y)[] GetNeighborChunks()
        {
            // TODO: Implement neighbor chunking
            return new[] { (0, 0) };
        }

        private (int x, int y)[] GetNeighborChunks(int x, int y)
        {
            // TODO: Implement neighbor chunking
            return new[] { (0, 0) };
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

        public void UnloadChunk(int x, int y)
        {
            var chunkCoordinate = GetChunkCoordinate(x, y);

            // Check that we are not unloading current or neighbor chunks
            if (CurrentChunk == chunkCoordinate) return;
            var neighborChunks = GetNeighborChunks();
            if (neighborChunks.Any(m => m.x == x && m.y == y)) return;

            _chunks.Remove(chunkCoordinate);
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
            // eg: 10_width * (27x / 10_width);
            // so (27x / 10_width) would round to 2 int so (10 * 2) = 20chunkX
            var chunkX = _width * (x / _width);
            var chunkY = _height * (y / _height);
            return (chunkX, chunkY);
        }
    }
}
