using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Venomaus.FlowVitae.Helpers
{
    /// <summary>
    /// Helpful comparer for <see cref="ValueTuple{T, T}"/>
    /// </summary>
    /// <typeparam name="T">struct, IComparable</typeparam>
    public class TupleComparer<T> : IEqualityComparer<ValueTuple<T, T>>
        where T : struct, IComparable<T>
    {
        /// <inheritdoc/>
        public bool Equals((T, T) x, (T, T) y)
        {
            return x.Item1.Equals(y.Item1) && x.Item2.Equals(y.Item2);
        }

        /// <inheritdoc/>
        public int GetHashCode([DisallowNull] ValueTuple<T, T> obj)
        {
            return Fnv1a.Hash32(obj.Item1, obj.Item2);
        }
    }
}
