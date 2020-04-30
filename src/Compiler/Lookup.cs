using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Roku.Compiler
{
    public static class Lookup
    {
        public static List<SourceCodeBody> AllPrograms(SourceCodeBody src)
        {
            var readed = new HashSet<SourceCodeBody>();

            List<SourceCodeBody> use_load(SourceCodeBody source)
            {
                var xs = new List<SourceCodeBody>();
                if (!readed.Contains(source))
                {
                    xs.Add(source);
                    readed.Add(source);
                }
                xs.AddRange(source.Uses.By<SourceCodeBody>().Map(xs => use_load(xs)).Flatten());
                return xs;
            }
            return use_load(src);
        }

        public static List<INamespace> AllNamespaces(SourceCodeBody src)
        {
            var readed = new HashSet<INamespace>();

            List<INamespace> use_load(INamespace source)
            {
                var xs = new List<INamespace>();
                if (!readed.Contains(source))
                {
                    xs.Add(source);
                    readed.Add(source);
                }
                if (source is SourceCodeBody body) xs.AddRange(body.Uses.Map(xs => use_load(xs)).Flatten());
                return xs;
            }
            return use_load(src);
        }

        public static IEnumerable<(FunctionBody Body, SourceCodeBody Source)> AllFunctionBodies(List<SourceCodeBody> srcs) => srcs.Map(x => AllFunctionBodies(x).Zip(Lists.Repeat(x))).Flatten();

        public static IEnumerable<FunctionBody> AllFunctionBodies(SourceCodeBody src) => src.Functions.By<FunctionBody>();

        public static IEnumerable<ExternFunction> AllExternFunctions(List<INamespace> srcs) => srcs.Map(AllFunctions<ExternFunction>).Flatten();

        public static IEnumerable<EmbeddedFunction> AllEmbeddedFunctions(List<INamespace> srcs) => srcs.Map(AllFunctions<EmbeddedFunction>).Flatten();

        public static IEnumerable<T> AllFunctions<T>(INamespace src) where T : IFunctionBody => src.Functions.By<T>();

        public static IFunctionBody? FindFunctionOrNull(INamespace ns, string name, List<IStructBody?> args) => ns is SourceCodeBody src ? FindFunctioInSourceCodeOrNulln(src, name, args) : FindFunctionInNamespaceOrNull(ns, name, args);

        public static IFunctionBody? FindFunctioInSourceCodeOrNulln(SourceCodeBody src, string name, List<IStructBody?> args)
        {
            var f = src.Functions.FindFirstOrNull(x => x.Name == name && FunctionArgumentsEquals(src, x, args));
            if (f is { }) return f;

            return src.Uses.Map(x => FindFunctionInNamespaceOrNull(x, name, args)).By<IFunctionBody>().FirstOrNull();
        }

        public static IFunctionBody? FindFunctionInNamespaceOrNull(INamespace ns, string name, List<IStructBody?> args) => ns.Functions.FindFirstOrNull(x => x.Name == name && FunctionArgumentsEquals(ns, x, args));

        public static bool FunctionArgumentsEquals(INamespace ns, IFunctionBody source, List<IStructBody?> args)
        {
            var fargs = GetArgumentsType(ns, source);
            if (fargs.Count != args.Count) return false;
            return fargs.Zip(args).And(x => TypeEquals(x.First, x.Second));
        }

        public static List<IStructBody> GetArgumentsType(INamespace ns, IFunctionBody body)
        {
            if (body is FunctionBody fb)
            {
                return fb.Arguments.Map(x => LoadStruct(ns, x.Type.Name)).ToList();
            }
            else if (body is EmbeddedFunction ef)
            {
                return ef.Arguments.Map(x => LoadStruct(ns, x.Name)).ToList();
            }
            else
            {
                var root = GetRootNamespace(ns);
                return body.Cast<ExternFunction>().Function.GetParameters().Map(x => LoadType(root, x.ParameterType).Cast<IStructBody>()).ToList();
            }
        }

        public static bool TypeEquals(IStructBody source, IStructBody? arg)
        {
            if (source is ExternStruct ea && arg is ExternStruct eb) return ea.Struct == eb.Struct;
            if (source is StructBody sa && arg is StructBody sb) return sa == sb;
            return false;
        }

        public static IStructBody LoadStruct(INamespace ns, string name)
        {
            var xs = LoadStructs(ns, name);
            if (!xs.IsNull()) return xs.First();

            if (ns is SourceCodeBody body)
            {
                return body.Uses.Map(x => LoadStructOrNull(x, name)).FindFirst(x => x is { })!;
            }

            throw new Exception();
        }

        public static IStructBody? LoadStructOrNull(INamespace ns, string name) => LoadStructs(ns, name).FirstOrNull();

        public static IEnumerable<IStructBody> LoadStructs(INamespace ns, string name) => ns.Structs.Where(x => x.Name == name);

        public static IEnumerable<ITypedValue> AllValues(IOperand op)
        {
            switch (op)
            {
                case Call x:
                    foreach (var arg in x.Function.Arguments) yield return arg;
                    if (x.Function.FirstLookup is { }) yield return x.Function.FirstLookup;
                    break;
            }
        }

        public static IEnumerable<LabelCode> AllLabels(FunctionBody f) => f.Body.By<LabelCode>().Unique();

        public static ExternStruct LoadType(RootNamespace root, Type t) => LoadType(root, t.Name, t);

        public static ExternStruct LoadType(RootNamespace root, string name, Type t) => LoadType(root, name, t.GetTypeInfo());

        public static ExternStruct LoadType(RootNamespace root, TypeInfo ti) => LoadType(root, ti.Name, ti);

        public static ExternStruct LoadType(RootNamespace root, string name, TypeInfo ti)
        {
            var st = root.Structs.Where(x => x is ExternStruct && x.Name == name).FirstOrNull();
            if (st is { }) return st.Cast<ExternStruct>();

            var t = new ExternStruct(name, ti);
            root.Structs.Add(t);
            return t;
        }

        public static RootNamespace GetRootNamespace(INamespace ns)
        {
            return ns is RootNamespace root ? root : GetRootNamespace(ns.Parent!);
        }

        public static ExternStruct? LoadTypeWithoutVoid(RootNamespace root, Type t) => LoadTypeWithoutVoid(root, t.GetTypeInfo());

        public static ExternStruct? LoadTypeWithoutVoid(RootNamespace root, TypeInfo ti) => ti == typeof(void) ? null : LoadType(root, ti.Name, ti);

        //public static RkCILFunction LoadFunction(RootNamespace root, MethodInfo method) => LoadFunction(root, method.Name, method);

        //public static RkCILFunction LoadFunction(RootNamespace root, string name, MethodInfo method)
        //{
        //    var f = new RkCILFunction(name, method);
        //    f.Return = LoadTypeWithoutVoid(root, method.ReturnType);
        //    method.GetParameters().Each(x => f.Arguments.Add(LoadType(root, x.ParameterType)));
        //    return f;
        //}
    }
}
