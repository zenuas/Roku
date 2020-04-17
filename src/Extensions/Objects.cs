using System;

namespace Extensions
{
    public static class Objects
    {
        public static R If<T, R>(this T self, Func<T, bool> cond, Func<T, R> then, Func<T, R> else_) => cond(self) ? then(self) : else_(self);

        public static T Then<T>(this T self, Func<T, bool> cond, Func<T, T> then) => cond(self) ? then(self) : self;

        public static T Else<T>(this T self, Func<T, bool> cond, Func<T, T> else_) => cond(self) ? self : else_(self);

        public static T Return<T>(this T self, Action<T> f)
        {
            f(self);
            return self;
        }

        public static T Cast<T>(this object self) => (T)self;
    }
}
