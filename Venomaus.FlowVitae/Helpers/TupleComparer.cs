using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Venomaus.FlowVitae.Helpers
{
    internal class TupleComparer<TCellType> : IEqualityComparer<ValueTuple<TCellType, TCellType>>
        where TCellType : struct, IComparable<TCellType>
    {
        public bool Equals((TCellType, TCellType) x, (TCellType, TCellType) y)
        {
            return x.Item1.Equals(y.Item1) && x.Item2.Equals(y.Item2);
        }

        public int GetHashCode([DisallowNull] ValueTuple<TCellType, TCellType> obj)
        {
            return Fnv1a.Hash32(obj.Item1, obj.Item2);
        }
    }
}
