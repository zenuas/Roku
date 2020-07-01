using System;
using System.Collections.Generic;

namespace Extensions
{
    public static class Cache
    {
        public static Func<T, R> Memoization<T, R>(this Func<T, R> f) where T : notnull
        {
            var memo = new Dictionary<T, R>();
            return x => memo.ContainsKey(x) ? memo[x] : (memo[x] = f(x));
        }

        public static Func<T1, T2, R> Memoization<T1, T2, R>(this Func<T1, T2, R> f)
        {
            var memo = new Dictionary<(T1, T2), R>();
            return (x, y) => memo.ContainsKey((x, y)) ? memo[(x, y)] : (memo[(x, y)] = f(x, y));
        }

        public static Func<T1, T2, T3, R> Memoization<T1, T2, T3, R>(this Func<T1, T2, T3, R> f)
        {
            var memo = new Dictionary<(T1, T2, T3), R>();
            return (x, y, z) => memo.ContainsKey((x, y, z)) ? memo[(x, y, z)] : (memo[(x, y, z)] = f(x, y, z));
        }
    }
}
