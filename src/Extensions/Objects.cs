using System;
using System.Diagnostics;
using System.Linq;

namespace Mina.Extensions;

public static class Objects
{
    [DebuggerHidden]
    public static R If<T, R>(this T self, Func<T, bool> cond, Func<T, R> then, Func<T, R> else_) => cond(self) ? then(self) : else_(self);

    [DebuggerHidden]
    public static T Then<T>(this T self, Func<T, bool> cond, Func<T, T> then) => cond(self) ? then(self) : self;

    [DebuggerHidden]
    public static T Else<T>(this T self, Func<T, bool> cond, Func<T, T> else_) => cond(self) ? self : else_(self);

    [DebuggerHidden]
    public static T Return<T>(this T self, Action<T> f)
    {
        f(self);
        return self;
    }

    [DebuggerHidden]
    public static R To<T, R>(this T self, Func<T, R> f) => f(self);

    [DebuggerHidden]
    public static bool In<T>(this T self, params T[] args) where T : IEquatable<T> => !args.Where(self.Equals).IsEmpty();

    [DebuggerHidden]
    public static T Cast<T>(this object self) => (T)self;
}
