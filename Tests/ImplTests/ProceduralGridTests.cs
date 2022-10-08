using Venomaus.FlowVitae.Basics.Procedural;
using Venomaus.Tests.TestObjects;

namespace Venomaus.Tests.ImplTests
{
    internal class ProceduralGridTests : BaseTests<int, TestCell<int>>
    {
        private const int Seed = 1000;
        protected override IProceduralGen<int, TestCell<int>>? ProcGen => new ProceduralGenerator<int, TestCell<int>>(Seed, GenerateChunk);

        private void GenerateChunk(Random random, int[] chunk, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    chunk[y * width + x] = random.Next(0, 10);
                }
            }
        }

        [Test]
        public void Cell_StoreState_ValueCorrect()
        {
            // Check if original cell is not 4
            var cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.Not.EqualTo(4));

            // Change cell to 4 with store state
            Grid.SetCell(new TestCell<int>(5, 5, 4, 10), true);

            // Verify if cell is 4 and number matches stored state
            cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(4));
            Assert.That(cell.Number, Is.EqualTo(10));

            // Set cell to 1 with no store state
            Grid.SetCell(5, 5, 1, false);

            // Verify if cell is 1 and number is default again
            cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(1));
            Assert.That(cell.Number, Is.EqualTo(default(int)));
        }

        [Test]
        public void Cell_Positive_Coordinate_Remapped_Correct()
        {
            Assert.That(Grid._chunkLoader, Is.Not.Null);

            Grid._chunkLoader.LoadChunk(55, 72);
            Grid.SetCell(55, 72, -5);
            var cell = Grid.GetCell(55, 72);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(-5));
        }

        [Test]
        public void Cell_Negative_Coordinate_Remapped_Correct()
        {
            Assert.That(Grid._chunkLoader, Is.Not.Null);

            Grid._chunkLoader.LoadChunk(-55, -72);
            Grid.SetCell(-55, -72, -5);
            var cell = Grid.GetCell(-55, -72);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(-5));
        }
    }
}
