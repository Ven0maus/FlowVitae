using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Venomaus.FlowVitae.Helpers
{
    internal class ConcurrentHashSet<T> : IEnumerable<T> where T : notnull
    {
        private readonly ConcurrentDictionary<T, byte> _dictionary = new ConcurrentDictionary<T, byte>();

        public ConcurrentHashSet(IEqualityComparer<T>? comparer = null)
        {
            _dictionary = new ConcurrentDictionary<T, byte>(comparer);
        }

        internal bool Add(T item)
        {
            return _dictionary.TryAdd(item, 0);
        }

        internal bool Remove(T item)
        {
            return _dictionary.TryRemove(item, out _);
        }

        internal bool Contains(T item)
        {
            return _dictionary.ContainsKey(item);
        }

        internal void Clear()
        {
            _dictionary.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var value in _dictionary)
                yield return value.Key;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }
    }
}
