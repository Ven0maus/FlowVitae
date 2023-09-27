using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Chunking;
using Venomaus.FlowVitae.Chunking.Generators;
using Venomaus.FlowVitae.Grids;

namespace Venomaus.UnitTests.Tests
{
    internal class BaseTests<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        protected Grid<TCellType, TCell> Grid { get; private set; }
        protected virtual IProceduralGen<TCellType, TCell>? ProcGen { get; }
        protected virtual Func<int, int, TCellType, TCell>? CustomConverter { get; }

        protected ChunkLoader<TCellType, TCell, IChunkData> ChunkLoader => Grid._chunkLoader ?? throw new Exception("Chunkloader null");

        protected int ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight;

        [SetUp]
        public virtual void Setup()
        {
            Grid = new Grid<TCellType, TCell>(ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight, ProcGen);
            Grid.SetCustomConverter(CustomConverter);
        }

        protected void AdjustProceduralGridGen(int seed, Action<Random, TCellType[], int, int, (int x, int y)> method)
        {
            var procGen = new ProceduralGenerator<TCellType, TCell>(seed, method);
            Grid = new Grid<TCellType, TCell>(ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight, procGen);
            Grid.SetCustomConverter(CustomConverter);
        }
    }
}
