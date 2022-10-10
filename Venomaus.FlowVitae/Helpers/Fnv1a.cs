using System.Linq;

namespace Venomaus.FlowVitae.Helpers
{
    internal static class Fnv1a
    {
        public static readonly int OffsetBasis32 = unchecked((int)2166136261);
        public static readonly int Prime32 = 16777619;

        public static readonly long OffsetBasis64 = unchecked((long)14695981039346656037);
        public static readonly long Prime64 = 1099511628211;

        public static int Hash32(params object[] objs)
        {
            return objs.Aggregate(OffsetBasis32, (r, o) => (r ^ o.GetHashCode()) * Prime32);
        }

        public static long Hash64(params object[] objs)
        {
            return objs.Aggregate(OffsetBasis64, (r, o) => (r ^ o.GetHashCode()) * Prime64);
        }
    }
}
