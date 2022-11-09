using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Chunking;
using Venomaus.FlowVitae.Chunking.Generators;

namespace Venomaus.FlowVitae.Grids
{
    /// <inheritdoc />
    public sealed class Grid<TCellType, TCell> : GridBase<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        /// <inheritdoc />
        public Grid(int width, int height) : base(width, height)
        { }

        /// <inheritdoc />
        public Grid(int viewPortWidth, int viewPortHeight, IProceduralGen<TCellType, TCell>? generator) : base(viewPortWidth, viewPortHeight, generator)
        { }

        /// <inheritdoc />
        public Grid(int viewPortWidth, int viewPortHeight, int chunkWidth, int chunkHeight, IProceduralGen<TCellType, TCell>? generator) : base(viewPortWidth, viewPortHeight, chunkWidth, chunkHeight, generator)
        { }
    }

    /// <summary>
    /// A basic high-performance memory efficient grid implementation
    /// </summary>
    /// <inheritdoc />
    public sealed class Grid<TCellType, TCell, TChunkData> : GridBase<TCellType, TCell, TChunkData>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
        where TChunkData : class, IChunkData
    {
        /// <inheritdoc />
        public Grid(int width, int height) : base(width, height)
        { }

        /// <inheritdoc />
        public Grid(int viewPortWidth, int viewPortHeight, int chunkWidth, int chunkHeight, IProceduralGen<TCellType, TCell, TChunkData>? generator)
            : base(viewPortWidth, viewPortHeight, chunkWidth, chunkHeight, generator)
        { }

        /// <inheritdoc />
        public Grid(int viewPortWidth, int viewPortHeight, IProceduralGen<TCellType, TCell, TChunkData>? generator)
            : base(viewPortWidth, viewPortHeight, generator)
        { }
    }
}