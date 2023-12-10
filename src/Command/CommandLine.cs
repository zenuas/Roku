using Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Command;

public static class CommandLine
{
    public static IEnumerable<(Attribute Attribute, MethodInfo Method)> GetCommands<T>()
    {
        foreach (var m in typeof(T).GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            foreach (var attr in m.GetCustomAttributes(true))
            {
                if (attr is CommandOptionAttribute o) yield return (o, m);
                else if (attr is CommandHelpAttribute h) yield return (h, m);
            }
        }

        foreach (var m in typeof(T).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty))
        {
            var setter = m.GetSetMethod(true)!;
            foreach (var attr in m.GetCustomAttributes(true))
            {
                if (attr is CommandOptionAttribute o) yield return (o, setter);
                else if (attr is CommandHelpAttribute h) yield return (h, setter);
            }
        }
    }

    public static string[] Parse<T>(T receiver, params string[] args)
    {
        var map = GetCommands<T>()
            .Where(x => x.Attribute is CommandOptionAttribute)
            .ToDictionary(x => x.Attribute.Cast<CommandOptionAttribute>().Command);

        var xargs = new List<string>();
        MethodInfo? method = null;
        var method_args = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--"))
            {
                method = map[args[i][2..]].Method;
            }
            else if (args[i].Length > 1 && args[i].StartsWith('-'))
            {
                method = map[args[i][1..2]].Method;
                if (args[i].Length > 2) method_args.Add(args[i][2..]);
            }
            else if (method is null)
            {
                xargs.Add(args[i]);
                continue;
            }
            else
            {
                method_args.Add(args[i]);
            }

            if (method is { } && method.GetParameters().Length <= method_args.Count)
            {
                var parameters = method.GetParameters();
                _ = method.Invoke(receiver, method_args.Select((arg, i) => Convert(parameters[i].ParameterType, arg)).ToArray());
                method = null;
                method_args.Clear();
            }
        }
        return [.. xargs];
    }

    public static (T Receiver, string[] Arguments) Run<T>(params string[] args)
    {
        var receiver = Expressions.GetNew<T>()();
        return (receiver, Run(receiver, args));
    }

    public static string[] Run<T>(T receiver, params string[] args) => Parse<T>(receiver, args);

    public static object Convert(Type t, string s)
    {
        return t switch
        {
            Type a when a == typeof(TextReader) => s == "-" ? Console.In : new StreamReader(s),
            Type a when a == typeof(StreamReader) => new StreamReader(s),
            Type a when a == typeof(TextWriter) => s == "-" ? Console.Out : new StreamWriter(s),
            Type a when a == typeof(StreamWriter) => new StreamWriter(s),
            Type a when a == typeof(byte) => byte.Parse(s),
            Type a when a == typeof(sbyte) => sbyte.Parse(s),
            Type a when a == typeof(int) => int.Parse(s),
            Type a when a == typeof(uint) => uint.Parse(s),
            Type a when a == typeof(short) => short.Parse(s),
            Type a when a == typeof(ushort) => ushort.Parse(s),
            Type a when a == typeof(long) => long.Parse(s),
            Type a when a == typeof(ulong) => ulong.Parse(s),
            Type a when a == typeof(float) => float.Parse(s),
            Type a when a == typeof(double) => double.Parse(s),
            Type a when a == typeof(char) => char.Parse(s),
            Type a when a == typeof(bool) => bool.Parse(s),
            Type a when a == typeof(decimal) => decimal.Parse(s),
            Type a when a == typeof(DateTime) => DateTime.Parse(s),
            _ => s,
        };
    }
}
