using Venomaus.FlowVitae.Basics;
using Venomaus.FlowVitae.Basics.Procedural;
using Venomaus.FlowVitae.Grids;

namespace Venomaus.Tests.ImplTests
{
    internal class BaseTests<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        protected Grid<TCellType, TCell> Grid { get; private set; }
        protected virtual IProceduralGen<TCellType, TCell>? ProcGen { get; }
        protected virtual Func<int, int, TCellType, TCell>? CustomConverter { get; }

        protected int ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight;

        [SetUp]
        public void Setup()
        {
            Grid = new Grid<TCellType, TCell>(ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight, ProcGen);;
            Grid.SetCustomConverter(CustomConverter);
        }

        protected void AdjustProceduralGridGen(int seed, Action<Random, TCellType[], int, int> method)
        {
            var procGen = new ProceduralGenerator<TCellType, TCell>(seed, method);
            Grid = new Grid<TCellType, TCell>(ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight, procGen);
        }
    }
}
