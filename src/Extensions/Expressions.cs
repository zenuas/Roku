using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace Extensions;

public static class Expressions
{
    public static Func<T, T, T> Add<T>() where T : IAdditionOperators<T, T, T> => (a, b) => a + b;

    public static Func<T, T, T> Subtract<T>() where T : ISubtractionOperators<T, T, T> => (a, b) => a - b;

    public static Func<T, T, T> Multiply<T>() where T : IMultiplyOperators<T, T, T> => (a, b) => a * b;

    public static Func<T, T, T> Divide<T>() where T : IDivisionOperators<T, T, T> => (a, b) => a / b;

    public static Func<T, T, T> Modulo<T>() where T : IModulusOperators<T, T, T> => (a, b) => a % b;

    public static Func<T, T, T> LeftShift<T>() where T : IShiftOperators<T, T, T> => (a, b) => a << b;

    public static Func<T, T, T> RightShift<T>() where T : IShiftOperators<T, T, T> => (a, b) => a >> b;

    public static Func<T, R> GetProperty<T, R>(string name) => GetFunction<T, R>(typeof(T).GetProperty(name)!.GetMethod!);

    public static Action<T, A> SetProperty<T, A>(string name) => GetAction<T, A>(typeof(T).GetProperty(name)!.SetMethod!);

    public static Func<T, R> GetFunction<T, R>(string name) => GetFunction<T, R>(typeof(T).GetMethod(name)!);

    public static Func<T, R> GetFunction<T, R>(MethodInfo method)
    {
        var receiver = Expression.Parameter(typeof(T));
        var call = Expression.Call(receiver, method);
        return Expression.Lambda<Func<T, R>>(
                method.ReturnParameter.ParameterType == typeof(R) ? call : Expression.Convert(call, typeof(R)),
                receiver
            ).Compile();
    }

    public static Action<T> GetAction<T>(MethodInfo method)
    {
        var receiver = Expression.Parameter(typeof(T));
        return Expression.Lambda<Action<T>>(
                Expression.Call(receiver, method),
                receiver
            ).Compile();
    }

    public static Action<T, A> GetAction<T, A>(MethodInfo method)
    {
        var receiver = Expression.Parameter(typeof(T));
        var arg1 = Expression.Parameter(typeof(A));
        return Expression.Lambda<Action<T, A>>(
                Expression.Call(receiver, method, arg1),
                receiver,
                arg1
            ).Compile();
    }

    public static Func<T> GetNew<T>()
    {
        return Expression.Lambda<Func<T>>(
                Expression.New(typeof(T).GetConstructor([])!)
            ).Compile();
    }
}
