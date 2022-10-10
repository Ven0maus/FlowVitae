using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Helpers;
using Venomaus.UnitTests.Tools;

namespace Venomaus.UnitTests.Tests
{
    internal class HelperTests
    {
        [Test]
        public void TupleComparerLong_Correct()
        {
            var comparer = new TupleComparer<long>();
            var tuple1 = ((long)5, (long)5);
            var tuple2 = ((long)2, (long)2);
            var tuple3 = ((long)5, (long)5);

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Equals(tuple1, tuple2), Is.Not.True);
                Assert.That(comparer.Equals(tuple1, tuple3), Is.True);
            });
        }

        [Test]
        public void TupleComparerInt_Correct()
        {
            var comparer = new TupleComparer<int>();
            var tuple1 = (5, 5);
            var tuple2 = (2, 2);
            var tuple3 = (5, 5);

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Equals(tuple1, tuple2), Is.Not.True);
                Assert.That(comparer.Equals(tuple1, tuple3), Is.True);
            });
        }

        [Test]
        public void TupleComparerByte_Correct()
        {
            var comparer = new TupleComparer<byte>();
            var tuple1 = ((byte)5, (byte)5);
            var tuple2 = ((byte)2, (byte)2);
            var tuple3 = ((byte)5, (byte)5);

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Equals(tuple1, tuple2), Is.Not.True);
                Assert.That(comparer.Equals(tuple1, tuple3), Is.True);
            });
        }

        [Test]
        public void CellFullComparer_Correct()
        {
            var comparer = new CellFullComparer<int>();
            var cell = new Cell<int>(5, 5, false, 10);
            var cell2 = new Cell<int>(5, 5, true, 10);
            var cell3 = new Cell<int>(5, 5, false, 10);
            var cell4 = new Cell<int>(5, 5, false, 8);

            var set = new Dictionary<Cell<int>, int>(comparer)
            {
                { cell, default }
            };

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Equals(cell, cell2), Is.Not.True);
                Assert.That(comparer.Equals(cell, cell3), Is.True);
                Assert.That(comparer.Equals(cell, cell4), Is.Not.True);
                Assert.That(comparer.Equals(cell, null), Is.Not.True);
                Assert.That(comparer.Equals(null, cell), Is.Not.True);
                Assert.That(comparer.Equals(null, null), Is.True);
                Assert.That(() => set.Add(cell3, default), Throws.Exception);
            });
        }

        [Test]
        public void CellWalkableComparer_Correct()
        {
            var comparer = new CellWalkableComparer<int>();
            var cell = new Cell<int>(5, 5, false, 10);
            var cell2 = new Cell<int>(5, 5, true, 10);
            var cell3 = new Cell<int>(5, 5, false, 8);
            var cell4 = new Cell<int>(5, 4, false, 10);

            var set = new Dictionary<Cell<int>, int>(comparer)
            {
                { cell, default }
            };

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Equals(cell, cell2), Is.Not.True);
                Assert.That(comparer.Equals(cell, cell3), Is.True);
                Assert.That(comparer.Equals(cell2, cell4), Is.Not.True);
                Assert.That(comparer.Equals(cell, null), Is.Not.True);
                Assert.That(comparer.Equals(null, cell), Is.Not.True);
                Assert.That(comparer.Equals(null, null), Is.True);
                Assert.That(() => set.Add(cell3, default), Throws.Exception);
            });
        }

        [Test]
        public void Fnv1a_DoesNotThrow_Exception()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => Fnv1a.Hash32(5, 5, 100), Throws.Nothing);
                Assert.That(() => Fnv1a.Hash64(5, 5, 100), Throws.Nothing);
            });
        }
    }
}
