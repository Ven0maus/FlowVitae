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
        protected bool ResizedViewport;

        protected Grid<TCellType, TCell> CreateNewGrid(int seed, Action<Random, TCellType[], int, int, (int x, int y)> method)
        {
            var procGen = new ProceduralGenerator<TCellType, TCell>(seed, method);
            var grid = CreateNewGrid(procGen);
            if (ResizedViewport)
            {
                grid.ResizeViewport(ViewPortWidth / 2, ViewPortHeight / 2);
            }
            return grid;
        }

        protected Grid<TCellType, TCell> CreateNewGrid(ProceduralGenerator<TCellType, TCell>? procGen = null)
        {
            var grid = new Grid<TCellType, TCell>(ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight, procGen ?? ProcGen);
            grid.SetCustomConverter(CustomConverter);
            if (ResizedViewport)
            {
                grid.ResizeViewport(ViewPortWidth / 2, ViewPortHeight / 2);
                grid.Center(grid.Width / 2, grid.Height / 2);
            }
            return grid;
        }
    }
}
