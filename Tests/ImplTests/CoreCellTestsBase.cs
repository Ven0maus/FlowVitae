using Venomaus.FlowVitae.Basics;
using Venomaus.FlowVitae.Grids;

namespace Venomaus.Tests.ImplTests
{
    internal class CoreCellTestsBase<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        protected Grid<TCellType, TCell> Grid;

        [SetUp]
        public void Setup()
        {
            Grid = new Grid<TCellType, TCell>(25, 25);
        }
    }
}
