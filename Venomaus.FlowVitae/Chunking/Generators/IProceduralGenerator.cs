using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Grids;

namespace Venomaus.FlowVitae.Chunking.Generators
{
    /// <summary>
    /// Interface for procedural generation that can be supplied to <see cref="GridBase{TCellType, TCell}"/>
    /// </summary>
    /// <typeparam name="TCellType"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    public interface IProceduralGen<TCellType, TCell> : IProceduralGen<TCellType, TCell, IChunkData>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    { }

    /// <summary>
    /// Interface for procedural generation that can be supplied to <see cref="GridBase{TCellType, TCell}"/>
    /// </summary>
    /// <typeparam name="TCellType"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    /// <typeparam name="TChunkData"></typeparam>
    public interface IProceduralGen<TCellType, TCell, TChunkData>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
        where TChunkData : class, IChunkData
    {
        /// <summary>
        /// The procedural generation seed
        /// </summary>
        int Seed { get; }

        /// <summary>
        /// Generates procedural content based on <paramref name="seed"/> for an area of (<paramref name="width"/>,<paramref name="height"/>)
        /// </summary>
        /// <param name="seed">Seed used to generate chunks (based on <see cref="Seed"/>)</param>
        /// <param name="width">Area width</param>
        /// <param name="height">Area height</param>
        /// <param name="chunkCoordinate">The most bottom-left coordinate of the chunk</param>
        /// <returns><typeparamref name="TCellType"/>[width*height]</returns>
        (TCellType[] chunkCells, TChunkData? chunkData) Generate(int seed, int width, int height, (int x, int y) chunkCoordinate);
    }
}
