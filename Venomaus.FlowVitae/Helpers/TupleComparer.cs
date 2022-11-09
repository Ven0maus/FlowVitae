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
        public bool Equals((T, T) obj1, (T, T) obj2)
        {
            return obj1.Item1.Equals(obj2.Item1) && obj1.Item2.Equals(obj2.Item2);
        }

        /// <inheritdoc/>
        public int GetHashCode([DisallowNull] ValueTuple<T, T> obj)
        {
            return Fnv1a.Hash32(obj.Item1, obj.Item2);
        }
    }
}
