using System;
using System.Diagnostics;
using System.Reflection;

namespace Extensions;

public static class Enums
{
    [DebuggerHidden]
    public static T? GetAttributeOrDefault<T>(this Enum e) where T : Attribute
    {
        var field = e.GetType().GetField(e.ToString());
        return field?.GetCustomAttribute<T>() is T attr ? attr : null;
    }
}
