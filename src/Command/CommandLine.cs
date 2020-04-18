using Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Command
{
    public static class CommandLine
    {
        public static IEnumerable<(Attribute Attribute, MethodInfo Method)> GetCommands<T>()
        {
            foreach (var m in typeof(T).GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach (var attr in m.GetCustomAttributes(true))
                {
                    if (attr is ShortOptionAttribute s) yield return (s, m);
                    else if (attr is LongOptionAttribute l) yield return (l, m);
                    else if (attr is CommandHelpAttribute h) yield return (h, m);
                }
            }

            foreach (var m in typeof(T).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty))
            {
                var setter = m.GetSetMethod(true)!;
                foreach (var attr in m.GetCustomAttributes(true))
                {
                    if (attr is ShortOptionAttribute s) yield return (s, setter);
                    else if (attr is LongOptionAttribute l) yield return (l, setter);
                    else if (attr is CommandHelpAttribute h) yield return (h, setter);
                }
            }
        }

        public static (string[] Arguments, (MethodInfo Method, string[] Arguments)[] Options) Parse<T>(params string[] args)
        {
            var map = GetCommands<T>()
                .Where(x => x.Attribute is ShortOptionAttribute || x.Attribute is LongOptionAttribute)
                .ToDictionary(x => x.Attribute is ShortOptionAttribute s ? s.Command.ToString() : x.Attribute.Cast<LongOptionAttribute>().Command);

            var xargs = new List<string>();
            var methods = new List<(MethodInfo, string[])>();
            MethodInfo? method = null;
            var method_args = new List<string>();

            for (var i = 0; i < args.Length; i++)
            {
                var is_method_name = false;
                if (args[i].StartsWith("--"))
                {
                    method = map[args[i].Substring(2)].Method;
                    is_method_name = true;
                }
                else if (args[i].StartsWith("-"))
                {
                    method = map[args[i].Substring(1, 1)].Method;
                    if (args[i].Length > 2) method_args.Add(args[i].Substring(2));
                    is_method_name = true;
                }

                if (method is null)
                {
                    xargs.Add(args[i]);
                }
                else
                {
                    if (!is_method_name) method_args.Add(args[i]);
                    if (method.GetParameters().Length() <= method_args.Count)
                    {
                        methods.Add((method, method_args.ToArray()));
                        method = null;
                        method_args.Clear();
                    }
                }
            }
            return (xargs.ToArray(), methods.ToArray());
        }

        public static string[] Run<T>(T receiver, params string[] args)
        {
            var (xargs, opt) = Parse<T>(args);
            opt.Each(x => x.Method.Invoke(receiver, x.Arguments));
            return xargs;
        }
    }
}
