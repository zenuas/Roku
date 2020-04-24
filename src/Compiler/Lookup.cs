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

        public static IEnumerable<ExternFunction> AllExternFunctions(List<INamespace> srcs) => srcs.Map(AllExternFunctions).Flatten();

        public static IEnumerable<ExternFunction> AllExternFunctions(INamespace src) => src.Functions.By<ExternFunction>();

        public static IFunctionBody? FindFunction(SourceCodeBody src, string name, List<ITypedValue> args)
        {
            var f = src.Functions.FindFirstOrNull(x => x.Name == name && FunctionArgumentsEquals(x, args));
            if (f is { }) return f;

            return src.Uses.Map(x => FindFunction(x, name, args)).By<IFunctionBody>().FirstOrNull();
        }

        public static IFunctionBody? FindFunction(INamespace ns, string name, List<ITypedValue> args) => ns.Functions.FindFirstOrNull(x => x.Name == name && FunctionArgumentsEquals(x, args));

        public static bool FunctionArgumentsEquals(IFunctionBody source, List<ITypedValue> args)
        {
            return true;
        }

        //public static bool TypeEquals(IType source, ITypedValue arg)
        //{
        //    return true;
        //}

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

        public static IEnumerable<ITypedValue> AllValues(Operand op)
        {
            switch (op)
            {
                case Call x:
                    foreach (var arg in x.Arguments) yield return arg;
                    if (x.FirstLookup is { }) yield return x.FirstLookup;
                    break;
            }
        }

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

        //public static RkCILStruct? LoadTypeWithoutVoid(RootNamespace root, Type t) => LoadTypeWithoutVoid(root, t.GetTypeInfo());

        //public static RkCILStruct? LoadTypeWithoutVoid(RootNamespace root, TypeInfo ti) => ti == typeof(void) ? null : LoadType(root, ti.Name, ti);

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
