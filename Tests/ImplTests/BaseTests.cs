using Venomaus.FlowVitae.Basics;
using Venomaus.FlowVitae.Basics.Procedural;
using Venomaus.FlowVitae.Grids;

namespace Venomaus.Tests.ImplTests
{
    internal class BaseTests<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        protected Grid<TCellType, TCell> Grid;
        protected virtual IProceduralGen<TCellType, TCell>? ProcGen { get; }
        protected virtual Func<int, int, TCellType, TCell>? CustomConverter { get; }

        [SetUp]
        public void Setup()
        {
            Grid = new Grid<TCellType, TCell>(25, 25, ProcGen);
            Grid.SetCustomConverter(CustomConverter);
        }
    }
}
