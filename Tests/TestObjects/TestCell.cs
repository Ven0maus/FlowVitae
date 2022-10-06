using Venomaus.FlowVitae.Cells;

namespace Venomaus.Tests.TestObjects
{
    internal class TestCell<TCellType> : Cell<TCellType>
        where TCellType : struct
    {
        public int Number { get; set; }

        public TestCell() : base()
        { }

        public TestCell(TCellType cellType, int number)
        {
            CellType = cellType;
            Number = number;
        }
    }
}
