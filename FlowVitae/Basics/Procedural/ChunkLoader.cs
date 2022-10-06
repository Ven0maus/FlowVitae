﻿using Venomaus.FlowVitae.Helpers;

namespace Venomaus.FlowVitae.Basics.Procedural
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
        private readonly Lazy<Dictionary<(int x, int y), Dictionary<(int x, int y), TCell>>> _modifiedCellsInChunks;

        private (int x, int y) _currentLoadedChunk;

        public ChunkLoader(int width, int height, IProceduralGen<TCellType, TCell> generator, Func<int, int, TCellType, TCell> cellTypeConverter)
        {
            _width = width;
            _height = height;
            _seed = generator.Seed;
            _generator = generator;
            _cellTypeConverter = cellTypeConverter;
            _chunks = new Dictionary<(int x, int y), TCellType[]>();
            _modifiedCellsInChunks = new Lazy<Dictionary<(int x, int y), Dictionary<(int x, int y), TCell>>>(
                () => new Dictionary<(int x, int y), Dictionary<(int x, int y), TCell>>());
            _currentLoadedChunk = (0, 0);
        }

        public TCell? GetChunkCell(int x, int y)
        {
            // TODO: Remove nullable when negative coords are supported
            if (x < 0 || y < 0) return null;

            var chunk = GetChunk(x, y, out var chunkCoordinate);
            if (chunk != null)
            {
                if (!InBounds(x, y)) return null;

                // Check if there are modified cell tiles within this chunk
                if (_modifiedCellsInChunks.IsValueCreated && _modifiedCellsInChunks.Value
                    .TryGetValue(chunkCoordinate, out var cells) &&
                    cells.TryGetValue((x, y), out var cell))
                {
                    // Return the modified cell if it exists
                    return cell;
                }

                // Return the non-modified cell
                return _cellTypeConverter(x, y, chunk[y * _width + x]);
            }
            return null;
        }

        public void SetChunkCell(int x, int y, TCell cell, bool storeState = false)
        {
            if (x < 0 || y < 0) return;

            var chunk = GetChunk(x, y, out var chunkCoordinate);
            if (chunk != null)
            {
                if (!InBounds(x, y)) return;

                if (!storeState && _modifiedCellsInChunks.IsValueCreated && _modifiedCellsInChunks.Value.TryGetValue(chunkCoordinate, out var storedCells))
                {
                    storedCells.Remove((x, y));
                }
                else if (storeState)
                {
                    // Check if there are modified cell tiles within this chunk
                    if (!_modifiedCellsInChunks.Value.TryGetValue(chunkCoordinate, out storedCells))
                    {
                        storedCells = new Dictionary<(int x, int y), TCell>();
                        _modifiedCellsInChunks.Value.Add(chunkCoordinate, storedCells);
                    }
                    storedCells[(x, y)] = cell;
                }
                
                // Adjust chunk cell & stored cell
                chunk[y * _width + x] = cell.CellType;
            }
        }

        public void SetCurrentChunk(int x, int y)
        {
            if (x < 0 || y < 0) return;

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
            if (x < 0 || y < 0) return;

            var coordinate = FindChunkCoordinates(x, y);

            if (!_chunks.ContainsKey(coordinate))
                _chunks.Add(coordinate, GenerateChunk(coordinate));
        }

        public void LoadChunk(TCell cell) => LoadChunk(cell.X, cell.Y);

        public void UnloadChunk(int x, int y)
        {
            if (x < 0 || y < 0) return;

            var chunkCoordinate = FindChunkCoordinates(x, y);

            _chunks.Remove(chunkCoordinate);
        }

        public void UnloadChunk(TCell cell) => UnloadChunk(cell.X, cell.Y);

        private bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < _width && y < _height;
        }

        private TCellType[]? GetChunk(int x, int y, out (int chunkX, int chunkY) chunkCoordinate)
        {
            if (x < 0 || y < 0)
            {
                chunkCoordinate = default;
                return null;
            }
            chunkCoordinate = FindChunkCoordinates(x, y);
            return _chunks.TryGetValue(chunkCoordinate, out var chunk) ? chunk : null;
        }

        private TCellType[] GenerateChunk((int x, int y) coordinate)
        {
            if (coordinate.x < 0 || coordinate.y < 0)
                throw new Exception($"Invalid coordinate: {coordinate.x}, {coordinate.y}");

            // Get a unique hash seed based on the chunk (x,y) and the main seed
            var chunkSeed = Fnv1a.Hash32(coordinate.x, coordinate.y, _seed);

            // Generate chunk data
            var chunk = _generator?.Generate(chunkSeed, _width, _height);
            if (chunk == null || chunk.Length > 0)
            {
                throw new Exception(chunk == null ? 
                    "Chunk generator returned null chunk data." : 
                    "Chunk generator returned empty chunk data.");
            }

            _chunks.Add(_currentLoadedChunk, chunk);

            return chunk;
        }

        private (int x, int y) FindChunkCoordinates(int x, int y)
        {
            if (x < 0 || y < 0) return default;
            // eg: 10_width * (27x / 10_width);
            // so (27x / 10_width) would round to 2 int so (10 * 2) = 20chunkX
            var chunkX = _width * (x / _width);
            var chunkY = _height * (y / _height);
            return (chunkX, chunkY);
        }
    }
}
