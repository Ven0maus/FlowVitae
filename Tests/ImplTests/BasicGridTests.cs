using Venomaus.Tests.TestObjects;

namespace Venomaus.Tests.ImplTests
{
    internal class BasicGridTests : BaseTests<int, TestCell<int>>
    {
        [Test]
        public void GetCell_Get_Correct()
        {
            var cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.EqualTo(new TestCell<int>(5, 5)));
        }

        [Test]
        public void SetCell_Set_Correct()
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
        public void SetCell_StoreState_Set_Correct()
        {
            var cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(0));

            var customCell = new TestCell<int>(5, 5, 1, 20);
            Grid.SetCell(customCell, true);

            var changedCell = Grid.GetCell(5, 5);
            Assert.That(changedCell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(changedCell.CellType, Is.EqualTo(customCell.CellType));
                Assert.That(changedCell.Number, Is.EqualTo(customCell.Number));
            });

            Grid.SetCell(5, 5, 1, true);

            changedCell = Grid.GetCell(5, 5);
            Assert.That(changedCell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(changedCell.CellType, Is.EqualTo(1));
                Assert.That(changedCell.Number, Is.EqualTo(default(int)));
            });
        }

        [Test]
        public void InBounds_Get_Correct()
        {
            var inBoundsTrue = Grid.InBounds(5, 5);
            Assert.That(inBoundsTrue, Is.EqualTo(true));

            var inBoundsFalse = Grid.InBounds(-5, 30);
            Assert.That(inBoundsFalse, Is.EqualTo(false));
        }

        [Test]
        public void ScreenToWorldPoint_Get_Correct()
        {
            var screenPosX = 5;
            var screenPosY = 5;
            var (x, y) = Grid.ScreenToWorldCoordinate(screenPosX, screenPosY);
            Assert.Multiple(() =>
            {
                Assert.That(screenPosX, Is.EqualTo(x));
                Assert.That(screenPosY, Is.EqualTo(y));
                Assert.That(() => Grid.ScreenToWorldCoordinate(Grid.Width + 5, Grid.Height + 5), Throws.Exception);
                Assert.That(() => Grid.ScreenToWorldCoordinate(-5, -5), Throws.Exception);
            });
        }

        [Test]
        public void WorldToScreenPoint_Get_Correct()
        {
            var worldPosX = 5;
            var worldPosY = 5;
            var (x, y) = Grid.WorldToScreenCoordinate(worldPosX, worldPosY);
            Assert.Multiple(() =>
            {
                Assert.That(worldPosX, Is.EqualTo(x));
                Assert.That(worldPosY, Is.EqualTo(y));
                Assert.That(() => Grid.WorldToScreenCoordinate(Grid.Width + 5, Grid.Height + 5), Throws.Exception);
                Assert.That(() => Grid.WorldToScreenCoordinate(-5, -5), Throws.Exception);
            });
        }

        [Test]
        public void GetViewPortCells_Get_Correct()
        {
            var cells = Grid.GetViewPortCells();
            Assert.That(cells, Has.Length.EqualTo(Grid.Width * Grid.Height));

            bool isValid = true;
            for (int x=0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Height; y++)
                {
                    var cell = Grid.GetCell(x, y);
                    isValid = cells[y * Grid.Width + x].Equals(cell);
                    if (!isValid)
                        break;
                }
            }
            Assert.That(isValid, Is.EqualTo(true));
        }
    }
}