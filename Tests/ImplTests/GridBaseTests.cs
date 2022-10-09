using Venomaus.FlowVitae.Basics;
using Venomaus.FlowVitae.Cells;

namespace Venomaus.Tests.ImplTests
{
    [TestFixture(25, 25, 25, 25)]
    [TestFixture(17, 28, 17, 28)]
    [TestOf(typeof(GridBase<int, Cell<int>>))]
    internal class GridBaseTests : BaseTests<int, Cell<int>>
    {
        public GridBaseTests(int viewPortWidth, int viewPortHeight, int chunkWidth, int chunkHeight)
        {
            ViewPortWidth = viewPortWidth;
            ViewPortHeight = viewPortHeight;
            ChunkWidth = chunkWidth;
            ChunkHeight = chunkHeight;
        }

        [Test]
        public void GetCell_Get_Correct()
        {
            var cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell, Is.EqualTo(new Cell<int>(5, 5)));

            cell = Grid.GetCell(-5, -5);
            Assert.That(cell, Is.Null);

            cell = Grid.GetCell(Grid.Width + 5, Grid.Height + 5);
            Assert.That(cell, Is.Null);
        }

        [Test]
        public void GetCells_Get_Correct()
        {
            var cellPositions = new[] { (5, 5), (3, 2), (4, 4) };
            var cells = Grid.GetCells(cellPositions).ToArray();
            Assert.That(cells, Has.Length.EqualTo(cellPositions.Length));

            for (int i = 0; i < cellPositions.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(cells[i].X, Is.EqualTo(cellPositions[i].Item1));
                    Assert.That(cells[i].Y, Is.EqualTo(cellPositions[i].Item2));
                });
            }
        }

        [Test]
        public void SetCells_GetCells_SetGet_Correct()
        {
            var cells = new[] 
            { 
                new Cell<int>(5, 5, false, 1), 
                new Cell<int>(3, 2, false, 2), 
                new Cell<int>(4, 4, true, 3) 
            };
            Assert.That(() => Grid.SetCells(cells, true), Throws.Nothing);

            var newCells = Grid.GetCells(cells.Select(a => (a.X, a.Y))).ToArray();
            Assert.That(newCells, Has.Length.EqualTo(cells.Length));

            for (int i = 0; i < cells.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(newCells[i].X, Is.EqualTo(cells[i].X));
                    Assert.That(newCells[i].Y, Is.EqualTo(cells[i].Y));
                    Assert.That(newCells[i].CellType, Is.EqualTo(cells[i].CellType), "Cell type is invalid");
                    Assert.That(newCells[i].Walkable, Is.EqualTo(cells[i].Walkable), "Cell number is invalid");
                });
            }

            Assert.That(() => Grid.SetCells(cells, false), Throws.Nothing);
            newCells = Grid.GetCells(cells.Select(a => (a.X, a.Y))).ToArray();
            Assert.That(newCells, Has.Length.EqualTo(cells.Length));

            for (int i = 0; i < cells.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(newCells[i].X, Is.EqualTo(cells[i].X));
                    Assert.That(newCells[i].Y, Is.EqualTo(cells[i].Y));
                    Assert.That(newCells[i].CellType, Is.EqualTo(cells[i].CellType), "Cell type is invalid");
                    Assert.That(newCells[i].Walkable, Is.EqualTo(true), "Cell number is invalid");
                });
            }
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
            Assert.Multiple(() =>
            {
                Assert.That(changedCell.CellType, Is.EqualTo(1));
                Assert.That(() => Grid.SetCell(Grid.Width + 5, Grid.Height + 5, 1), Throws.Nothing);
                Assert.That(() => Grid.SetCell(-5, -5, 1), Throws.Nothing);
            });
        }

        [Test]
        public void GetCellType_Get_Correct()
        {
            var cellType = Grid.GetCellType(Grid.Width / 2, Grid.Height / 2);
            Assert.That(cellType, Is.Not.EqualTo(-1));

            Grid.SetCell(Grid.Width / 2, Grid.Height / 2, -1);

            cellType = Grid.GetCellType(Grid.Width / 2, Grid.Height / 2);
            Assert.That(cellType, Is.EqualTo(-1));
        }

        [Test]
        public void StoreState_SetAndGet_Correct()
        {
            var cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(0));

            var customCell = new Cell<int>(5, 5, false, 20);
            Grid.SetCell(customCell, true);

            var changedCell = Grid.GetCell(5, 5);
            Assert.That(changedCell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(changedCell.CellType, Is.EqualTo(customCell.CellType));
                Assert.That(changedCell.Walkable, Is.EqualTo(customCell.Walkable));
            });

            Grid.SetCell(5, 5, 1, true);

            changedCell = Grid.GetCell(5, 5);
            Assert.That(changedCell, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(changedCell.CellType, Is.EqualTo(1));
                Assert.That(changedCell.Walkable, Is.EqualTo(true));
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
                Assert.That(x, Is.EqualTo(screenPosX));
                Assert.That(y, Is.EqualTo(screenPosY));
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
                Assert.That(x, Is.EqualTo(worldPosX));
                Assert.That(y, Is.EqualTo(worldPosY));
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

        [Test]
        public void IsWorldCoordinateOnViewPort_Get_Correct()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Grid.IsWorldCoordinateOnViewPort(5, 5), Is.EqualTo(true));
                Assert.That(Grid.IsWorldCoordinateOnViewPort(-5, -5), Is.EqualTo(false));
                Assert.That(Grid.IsWorldCoordinateOnViewPort(Grid.Width + 5, Grid.Height + 5), Is.EqualTo(false));
            });
        }

        [Test]
        public void OnCellUpdate_Raised_Correct()
        {
            object? sender = null;
            CellUpdateArgs<int, Cell<int>>? args = null;
            Grid.OnCellUpdate += (cellSender, cellArgs) =>
            {
                sender = cellSender;
                args = cellArgs;
            };

            Grid.SetCell(new Cell<int>(5, 5, false, -1));

            Assert.That(args, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(sender, Is.Null);
                Assert.That(args.ScreenX, Is.EqualTo(5));
                Assert.That(args.ScreenY, Is.EqualTo(5));
                Assert.That(args.Cell.X, Is.EqualTo(5));
                Assert.That(args.Cell.Y, Is.EqualTo(5));
                Assert.That(args.Cell.CellType, Is.EqualTo(-1));
                Assert.That(args.Cell.Walkable, Is.EqualTo(false));
            });
        }

        [Test]
        public void SetCustomConverter_Converts_Correct()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => Grid.SetCustomConverter((int x, int y, int cellType) =>
                {
                    return new Cell<int>(x, y, cellType != -1, cellType);
                }), Throws.Nothing);

                Assert.That(() => Grid.SetCell(5, 5, -1), Throws.Nothing);
            });

            var cell = Grid.GetCell(5, 5);

            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.Walkable, Is.EqualTo(false));
        }
    }
}