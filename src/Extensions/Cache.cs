using System;
using System.Collections.Generic;

namespace Extensions;

public static class Cache
{
    public static Func<T, R> Memoization<T, R>(this Func<T, R> f) where T : notnull
    {
        var memo = new Dictionary<T, R>();
        return x => memo.TryGetValue(x, out var value) ? value : (memo[x] = f(x));
    }

    public static Func<T1, T2, R> Memoization<T1, T2, R>(this Func<T1, T2, R> f)
    {
        var memo = new Dictionary<(T1, T2), R>();
        return (x, y) => memo.TryGetValue((x, y), out var value) ? value : (memo[(x, y)] = f(x, y));
    }

    public static Func<T1, T2, T3, R> Memoization<T1, T2, T3, R>(this Func<T1, T2, T3, R> f)
    {
        var memo = new Dictionary<(T1, T2, T3), R>();
        return (x, y, z) => memo.TryGetValue((x, y, z), out var value) ? value : (memo[(x, y, z)] = f(x, y, z));
    }
}
