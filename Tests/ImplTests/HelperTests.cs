using Venomaus.FlowVitae.Helpers;

namespace Venomaus.Tests.ImplTests
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
