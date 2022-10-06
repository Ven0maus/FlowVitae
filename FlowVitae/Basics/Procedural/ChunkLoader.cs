using Venomaus.FlowVitae.Helpers;

namespace Venomaus.FlowVitae.Basics.Procedural
{
    internal class ChunkLoader<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        private readonly int _seed;
        private readonly int _width, _height;
        private readonly IProceduralGen<TCellType, TCell> _generator;
        private readonly Dictionary<(int x, int y), TCellType[]> _chunks;
        private (int x, int y) _currentLoadedChunk;

        public ChunkLoader(int width, int height, int seed, IProceduralGen<TCellType, TCell> generator)
        {
            _width = width;
            _height = height;
            _seed = seed;
            _generator = generator;
            _chunks = new Dictionary<(int x, int y), TCellType[]>();
            _currentLoadedChunk = (0, 0);
        }

        public void SetCurrentChunk(int x, int y)
        {
            var chunkX = FindChunkAxisFast(x, _width);
            var chunkY = FindChunkAxisFast(y, _height);
            var coordinate = (chunkX, chunkY);

            if (_chunks.ContainsKey(coordinate))
                _currentLoadedChunk = (x, y);
        }

        public void LoadChunk(int x, int y)
        {
            if (x < 0 || y < 0) return;

            var chunkX = FindChunkAxisFast(x, _width);
            var chunkY = FindChunkAxisFast(y, _height);
            var coordinate = (chunkX, chunkY);

            if (!_chunks.ContainsKey(coordinate))
                _chunks.Add(coordinate, GenerateChunk(coordinate));
        }

        public void LoadChunk(TCell cell) => LoadChunk(cell.X, cell.Y);

        public void UnloadChunk(int x, int y)
        {
            if (x < 0 || y < 0) return;

            var chunkX = FindChunkAxisFast(x, _width);
            var chunkY = FindChunkAxisFast(y, _height);

            _chunks.Remove((chunkX, chunkY));
        }

        public void UnloadChunk(TCell cell) => UnloadChunk(cell.X, cell.Y);
        
        private TCellType[] GenerateChunk((int x, int y) coordinate)
        {
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

        private static int FindChunkAxisFast(int chunkCoord, int coordSize)
        {
            return (int)Math.Ceiling((decimal)chunkCoord / coordSize);
        }

        private static int FindChunkAxisSlow(int chunkCoord, int coordSize)
        {
            // TODO: Test performance
            int currentChunkCalcX = 0;
            while (chunkCoord > currentChunkCalcX)
                currentChunkCalcX += coordSize;
            return currentChunkCalcX;
        }
    }
}
