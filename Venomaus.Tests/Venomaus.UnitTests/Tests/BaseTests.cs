using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Chunking.Generators;
using Venomaus.FlowVitae.Grids;

namespace Venomaus.UnitTests.Tests
{
    internal class BaseTests<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        protected virtual IProceduralGen<TCellType, TCell>? ProcGen { get; }
        protected virtual Func<int, int, TCellType, TCell>? CustomConverter { get; }

        protected int ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight;

        protected Grid<TCellType, TCell> CreateNewGrid(int seed, Action<Random, TCellType[], int, int, (int x, int y)> method)
        {
            var procGen = new ProceduralGenerator<TCellType, TCell>(seed, method);
            var grid = CreateNewGrid(procGen);
            return grid;
        }

        protected Grid<TCellType, TCell> CreateNewGrid(ProceduralGenerator<TCellType, TCell>? procGen = null)
        {
            var grid = new Grid<TCellType, TCell>(ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight, procGen ?? ProcGen);
            grid.SetCustomConverter(CustomConverter);
            return grid;
        }
    }
}
