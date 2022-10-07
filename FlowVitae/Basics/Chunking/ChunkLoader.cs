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

        private (int x, int y) _currentLoadedChunk;

        public ChunkLoader(int width, int height, IProceduralGen<TCellType, TCell> generator, Func<int, int, TCellType, TCell> cellTypeConverter)
        {
            _width = width;
            _height = height;
            _seed = generator.Seed;
            _generator = generator;
            _cellTypeConverter = cellTypeConverter;
            _chunks = new Dictionary<(int x, int y), TCellType[]>();
            _currentLoadedChunk = (0, 0);
        }

        public (int x, int y) RemapChunkCoordinate(int x, int y)
        {
            _ = GetChunk(x, y, out var chunkCoordinate);
            return RemapChunkCoordinate(x, y, chunkCoordinate);
        }

        private static (int x, int y) RemapChunkCoordinate(int x, int y, (int x, int y) chunkCoordinate)
        {
            return (x: Math.Abs(x - chunkCoordinate.x), y: Math.Abs(y - chunkCoordinate.y));
        }

        public void LoadChunksAround(int x, int y, bool includeSourceChunk)
        {
            var chunkCoordinate = FindChunkCoordinates(x, y);
            if (includeSourceChunk)
                LoadChunk(chunkCoordinate.x, chunkCoordinate.y);
            // TODO: LoadNeighborChunks(chunkCoordinate.x, chunkCoordinate.y);
        }

        public TCell? GetChunkCell(int x, int y)
        {
            var chunk = GetChunk(x, y, out var chunkCoordinate);
            if (chunk != null)
            {
                var remappedCoordinate = RemapChunkCoordinate(x, y, chunkCoordinate);

                // Check if there are modified cell tiles within this chunk
                if (_modifiedCellsInChunks != null && _modifiedCellsInChunks
                    .TryGetValue(chunkCoordinate, out var cells) &&
                    cells.TryGetValue(remappedCoordinate, out var cell))
                {
                    // Return the modified cell if it exists
                    return cell;
                }

                // Return the non-modified cell
                return _cellTypeConverter(remappedCoordinate.x, remappedCoordinate.y, 
                    chunk[remappedCoordinate.y * _width + remappedCoordinate.x]);
            }
            return null;
        }

        public void SetChunkCell(int x, int y, TCell cell, bool storeState = false)
        {
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

                // Adjust chunk cell & stored cell
                chunk[remappedCoordinate.y * _width + remappedCoordinate.x] = cell.CellType;
            }
        }

        public void SetCurrentChunk(int x, int y)
        {
            var coordinate = FindChunkCoordinates(x, y);

            if (_chunks.ContainsKey(coordinate))
                _currentLoadedChunk = (x, y);
        }

        public TCellType[] GetCurrentChunk()
        {
            if (!_chunks.TryGetValue(_currentLoadedChunk, out var chunk))
                throw new Exception("No chunk loaded, initialize chunks first!");
            return chunk;
        }

        public TCellType[] GetNeighborChunk(Direction direction)
        {
            // TODO: Implement neighbor chunking
            // TODO: Support negative coordinates
            return new TCellType[_width * _height];
        }

        public void LoadChunk(int x, int y)
        {
            var coordinate = FindChunkCoordinates(x, y);

            if (!_chunks.ContainsKey(coordinate))
                GenerateChunk(coordinate);
        }

        public void LoadChunk(TCell cell) => LoadChunk(cell.X, cell.Y);

        public void UnloadChunk(int x, int y)
        {
            var chunkCoordinate = FindChunkCoordinates(x, y);

            _chunks.Remove(chunkCoordinate);
        }

        public void UnloadChunk(TCell cell) => UnloadChunk(cell.X, cell.Y);

        private TCellType[]? GetChunk(int x, int y, out (int x, int y) chunkCoordinate)
        {
            chunkCoordinate = FindChunkCoordinates(x, y);
            return _chunks.TryGetValue(chunkCoordinate, out var chunk) ? chunk : null;
        }

        private TCellType[] GenerateChunk((int x, int y) coordinate)
        {
            // Get a unique hash seed based on the chunk (x,y) and the main seed
            var chunkSeed = Fnv1a.Hash32(coordinate.x, coordinate.y, _seed);

            // Generate chunk data
            var chunk = _generator?.Generate(chunkSeed, _width, _height);
            if (chunk == null || chunk.Length == 0)
            {
                throw new Exception(chunk == null ?
                    "Chunk generator returned null chunk data." :
                    "Chunk generator returned empty chunk data.");
            }

            _chunks.Add(coordinate, chunk);

            return chunk;
        }

        private (int x, int y) FindChunkCoordinates(int x, int y)
        {
            // eg: 10_width * (27x / 10_width);
            // so (27x / 10_width) would round to 2 int so (10 * 2) = 20chunkX
            var chunkX = _width * (x / _width);
            var chunkY = _height * (y / _height);
            return (chunkX, chunkY);
        }
    }
}
