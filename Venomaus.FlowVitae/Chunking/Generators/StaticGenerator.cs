using System;
using Venomaus.FlowVitae.Cells;

namespace Venomaus.FlowVitae.Chunking.Generators
{
    /// <summary>
    /// Basic static generator implementation
    /// </summary>
    /// <typeparam name="TCellType">The cell type to be used within the <see cref="IProceduralGen{TCellType, TCell}"/></typeparam>
    /// <typeparam name="TCell">The wrapper object used to wrap around the cell type</typeparam>
    public sealed class StaticGenerator<TCellType, TCell> : IProceduralGen<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        /// <summary>
        /// Seed is always default value for static generation
        /// </summary>
        public int Seed { get; }

        private readonly TCellType[] _baseMap;
        private readonly int _width, _height;
        private readonly TCellType _nullCell;

        /// <summary>
        /// Constructor for static generation
        /// </summary>
        /// <param name="baseMap">The static base map</param>
        /// <param name="width">The width of the baseMap</param>
        /// <param name="height">The height of the baseMap</param>
        /// <param name="outOfBoundsCellType">The <typeparamref name="TCellType"/> to be used when a chunk has cells that don't fit the base map.</param>
        public StaticGenerator(TCellType[] baseMap, int width, int height, TCellType outOfBoundsCellType)
        {
            _baseMap = baseMap;
            _width = width;
            _height = height;
            _nullCell = outOfBoundsCellType;
        }

        /// <summary>
        /// Generates static content based on <see cref="_baseMap"/> for an area of (<paramref name="width"/>,<paramref name="height"/>)
        /// </summary>
        /// <param name="seed">A unique seed for this chunk, mostly not used in static generation</param>
        /// <param name="width">Area width</param>
        /// <param name="height">Area height</param>
        /// <param name="chunkCoordinate">The most bottom-left coordinate of the chunk</param>
        /// <returns><typeparamref name="TCellType"/>[width*height]</returns>
        public (TCellType[] chunkCells, IChunkData? chunkData) Generate(int seed, int width, int height, (int x, int y) chunkCoordinate)
        {
            var chunk = new TCellType[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var targetX = chunkCoordinate.x + x;
                    var targetY = chunkCoordinate.y + y;
                    chunk[y * width + x] = InBounds(x, y) ? _baseMap[targetY * width + targetX] : _nullCell;
                }
            }
            return (chunk, null);
        }

        private bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < _width && y < _height;
        }
    }

    /// <summary>
    /// Basic static generator implementation
    /// </summary>
    /// <typeparam name="TCellType">The cell type to be used within the <see cref="IProceduralGen{TCellType, TCell}"/></typeparam>
    /// <typeparam name="TCell">The wrapper object used to wrap around the cell type</typeparam>
    /// <typeparam name="TChunkData">The chunk data container object</typeparam>
    public sealed class StaticGenerator<TCellType, TCell, TChunkData> : IProceduralGen<TCellType, TCell, TChunkData>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
        where TChunkData : class, IChunkData
    {
        /// <summary>
        /// Seed is always default value for static generation
        /// </summary>
        public int Seed { get; }

        private readonly TCellType[] _baseMap;
        private readonly int _width, _height;
        private readonly TCellType _nullCell;
        private readonly Func<int, TCellType[], int, int, (int x, int y), TChunkData> _method;

        /// <summary>
        /// Constructor for static generation, method param uses following signature: (seed, <typeparamref name="TCellType"/>[], width, height, chunkCoordinate)
        /// </summary>
        /// <param name="baseMap">The static base map</param>
        /// <param name="width">The width of the baseMap</param>
        /// <param name="height">The height of the baseMap</param>
        /// <param name="outOfBoundsCellType">The <typeparamref name="TCellType"/> to be used when a chunk has cells that don't fit the base map.</param>
        /// <param name="method">Signature: (seed, <typeparamref name="TCellType"/>[], width, height, chunkCoordinate)</param>
        public StaticGenerator(TCellType[] baseMap, int width, int height, TCellType outOfBoundsCellType, Func<int, TCellType[], int, int, (int x, int y), TChunkData> method)
        {
            _baseMap = baseMap;
            _width = width;
            _height = height;
            _method = method;
            _nullCell = outOfBoundsCellType;
        }

        /// <summary>
        /// Generates static content based on <see cref="_baseMap"/> for an area of (<paramref name="width"/>,<paramref name="height"/>)
        /// </summary>
        /// <param name="seed">A unique seed for this chunk, mostly not used in static generation</param>
        /// <param name="width">Area width</param>
        /// <param name="height">Area height</param>
        /// <param name="chunkCoordinate">The most bottom-left coordinate of the chunk</param>
        /// <returns><typeparamref name="TCellType"/>[width*height]</returns>
        public (TCellType[] chunkCells, TChunkData? chunkData) Generate(int seed, int width, int height, (int x, int y) chunkCoordinate)
        {
            var chunk = new TCellType[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var targetX = chunkCoordinate.x + x;
                    var targetY = chunkCoordinate.y + y;
                    chunk[y * width + x] = InBounds(x, y) ? _baseMap[targetY * width + targetX] : _nullCell;
                }
            }
            return (chunk, _method(seed, chunk, width, height, chunkCoordinate));
        }

        private bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < _width && y < _height;
        }
    }
}
