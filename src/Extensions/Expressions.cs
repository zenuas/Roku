using System;
using System.Linq.Expressions;

namespace Extensions
{
    public class Expressions
    {
        public static Func<T, T, T> Add<T>()
        {
            var x = Expression.Parameter(typeof(T));
            var y = Expression.Parameter(typeof(T));
            return Expression.Lambda<Func<T, T, T>>(Expression.Add(x, y), x, y).Compile();
        }

        public static Func<T, T, T> Subtract<T>()
        {
            var x = Expression.Parameter(typeof(T));
            var y = Expression.Parameter(typeof(T));
            return Expression.Lambda<Func<T, T, T>>(Expression.Subtract(x, y), x, y).Compile();
        }

        public static Func<T, T, T> Multiply<T>()
        {
            var x = Expression.Parameter(typeof(T));
            var y = Expression.Parameter(typeof(T));
            return Expression.Lambda<Func<T, T, T>>(Expression.Multiply(x, y), x, y).Compile();
        }

        public static Func<T, T, T> Divide<T>()
        {
            var x = Expression.Parameter(typeof(T));
            var y = Expression.Parameter(typeof(T));
            return Expression.Lambda<Func<T, T, T>>(Expression.Divide(x, y), x, y).Compile();
        }
    }
}
