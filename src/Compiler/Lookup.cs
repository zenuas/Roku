using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using Roku.TypeSystem;
using System;
using System.Collections.Generic;

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

        public static IFunction? FindFunction(SourceCodeBody src, string name, List<ITypedValue> args)
        {
            var f = src.Functions.FindFirstOrNull(x => x.Function.Name == name && FunctionArgumentsEquals(x.Function.Arguments, args));
            if (f is { }) return f.Function;

            return src.Uses.Map(x => FindFunction(x, name, args)).By<IFunction>().FirstOrNull();
        }

        public static IFunction? FindFunction(INamespace ns, string name, List<ITypedValue> args) => ns.Functions.FindFirstOrNull(x => x.Function.Name == name && FunctionArgumentsEquals(x.Function.Arguments, args))?.Function;

        public static bool FunctionArgumentsEquals(List<IType> source, List<ITypedValue> args) => source.Count != args.Count ? false : source.Zip(args).And(x => TypeEquals(x.First, x.Second));

        public static bool TypeEquals(IType source, ITypedValue arg)
        {
            return true;
        }

        public static IType LoadStruct(INamespace ns, string name)
        {
            var xs = LoadStructs(ns, name);
            if (!xs.IsNull()) return xs.First();

            if (ns is SourceCodeBody body)
            {
                return body.Uses.Map(x => LoadStructOrNull(x, name)).FindFirst(x => x is { })!;
            }

            throw new Exception();
        }

        public static IType? LoadStructOrNull(INamespace ns, string name) => LoadStructs(ns, name).FirstOrNull();

        public static IEnumerable<IType> LoadStructs(INamespace ns, string name) => ns.Structs.Map(x => x.Struct).Where(x => x.Name == name);

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
    }
}
