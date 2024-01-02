using System.Collections.Generic;
using System.Diagnostics;

namespace Mina.Extensions;

public static class Dictionaries
{
    [DebuggerHidden]
    public static TValue GetOrNew<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key)
        where TKey : notnull
        where TValue : new()
    {
        if (!self.ContainsKey(key)) self.Add(key, new());
        return self[key];
    }

    [DebuggerHidden]
    public static TValue? GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key)
        where TKey : notnull
        where TValue : notnull
    {
        return self.TryGetValue(key, out var value) ? value : default;
    }
}
