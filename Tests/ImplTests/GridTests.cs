using Venomaus.FlowVitae.Cells;

namespace Venomaus.Tests.ImplTests
{
    internal class GridTests : BaseTests<int, Cell<int>>
    {
        [Test]
        public void GetCell()
        {
            var cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.EqualTo(new Cell<int>(5, 5)));
        }

        [Test]
        public void SetCell()
        {
            var cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(0));

            Grid.SetCell(5, 5, 1);

            var changedCell = Grid.GetCell(5, 5);
            Assert.That(changedCell, Is.Not.Null);
            Assert.That(changedCell.CellType, Is.EqualTo(1));
        }

        [Test]
        public void InBounds()
        {
            var inBoundsTrue = Grid.InBounds(5, 5);
            Assert.That(inBoundsTrue, Is.EqualTo(true));

            var inBoundsFalse = Grid.InBounds(-5, 30);
            Assert.That(inBoundsFalse, Is.EqualTo(false));
        }
    }
}