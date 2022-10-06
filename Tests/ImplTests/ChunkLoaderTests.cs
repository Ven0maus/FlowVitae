using Venomaus.FlowVitae.Basics.Procedural;
using Venomaus.Tests.TestObjects;

namespace Venomaus.Tests.ImplTests
{
    internal class ChunkLoaderTests : BaseTests<int, TestCell<int>>
    {
        private const int Seed = 1000;
        protected override IProceduralGen<int, TestCell<int>>? ProcGen => new ProceduralGenerator<TestCell<int>>(Seed);

        [Test]
        public void SetCell_StoreState_ValueCorrect()
        {
            var random = new Random(Seed);
            var number = random.Next();

            // Check if original cell is not 4
            var cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.Not.EqualTo(4));

            // Change cell to 4 with store state
            Grid.SetCell(5, 5, new TestCell<int>(4, number), true);

            // Verify if cell is 4 and number matches stored state
            cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(4));
            Assert.That(cell.Number, Is.EqualTo(number));

            // Set cell to 1 with no store state
            Grid.SetCell(5, 5, 1, false);

            // Verify if cell is 1 and number is default again
            cell = Grid.GetCell(5, 5);
            Assert.That(cell, Is.Not.Null);
            Assert.That(cell.CellType, Is.EqualTo(1));
            Assert.That(cell.Number, Is.EqualTo(default(int)));
        }
    }
}
