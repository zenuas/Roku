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

        public static FunctionCaller? FindFunctionOrNull(INamespace ns, string name, List<IStructBody?> args) => ns is SourceCodeBody src ? FindFunctioInSourceCodeOrNull(src, name, args) : FindFunctionInNamespaceOrNull(ns, name, args);

        public static FunctionCaller? FindFunctioInSourceCodeOrNull(SourceCodeBody src, string name, List<IStructBody?> args)
        {
            var x = FindFunctionInNamespaceOrNull(src, name, args);
            if (x is { }) return x;

            return src.Uses.Map(x => FindFunctionInNamespaceOrNull(x, name, args)).By<FunctionCaller>().FirstOrNull();
        }

        public static FunctionCaller? FindFunctionInNamespaceOrNull(INamespace ns, string name, List<IStructBody?> args)
        {
            foreach (var x in ns.Functions.Where(x => x.Name == name))
            {
                var v = FunctionArgumentsEquals(ns, x, args);
                if (v.Item1) return new FunctionCaller(x, v.Item2);
            }
            return null;
        }

        public static (bool, GenericsMapper) FunctionArgumentsEquals(INamespace ns, IFunctionBody source, List<IStructBody?> args)
        {
            var gens = ApplyArgumentsToGenericsParameter(source, args);
            var fargs = GetArgumentsType(ns, source, gens);
            return (fargs.Count != args.Count ? false : fargs.Zip(args).And(x => TypeEquals(x.First, x.Second)), gens);
        }

        public static List<IStructBody?> GetArgumentsType(INamespace ns, IFunctionBody body, GenericsMapper gens) => FunctionToArgumentsType(body).Map(x => GetArgumentType(ns, x, gens)).ToList();

        public static IStructBody? GetArgumentType(INamespace ns, ITypeDefinition t, GenericsMapper gens)
        {
            if (t is TypeValue gen && gen.Types == Types.Generics)
            {
                return gens[gen]!;
            }
            else if (t is TypeValue tv)
            {
                return LoadStruct(ns, tv.Name);
            }
            else if (t is TypeInfoValue ti)
            {
                return LoadType(GetRootNamespace(ns), ti.Type);
            }
            throw new Exception();
        }

        public static IEnumerable<ITypeDefinition> FunctionToArgumentsType(IFunctionBody body)
        {
            if (body is FunctionBody fb)
            {
                return fb.Arguments.Map(x => x.Type);
            }
            else if (body is EmbeddedFunction ef)
            {
                return ef.Arguments;
            }
            else if (body is ExternFunction xf)
            {
                return xf.Function.GetParameters().Map(x => new TypeInfoValue(x.ParameterType));
            }
            throw new Exception();
        }

        public static IEnumerable<TypeValue> ExtractArgumentsTypeToGenericsParameter(IEnumerable<ITypeDefinition> types) => types.By<TypeValue>().Where(x => x.Types == Types.Generics).Unique();

        public static GenericsMapper ApplyArgumentsToGenericsParameter(IFunctionBody body, List<IStructBody?> args)
        {
            var param = FunctionToArgumentsType(body).ToList();
            var gens = new GenericsMapper();
            ExtractArgumentsTypeToGenericsParameter(param).Each(x => gens[x] = null);

            Action<ITypeDefinition, IStructBody?> match = (p, arg) =>
            {
                if (p is TypeValue v && v.Types == Types.Generics) gens[v] = arg;
            };
            param.Each((x, i) => match(x, args.Count > i ? args[i] : null));

            return gens;
        }

        public static bool TypeEquals(IStructBody? source, IStructBody? arg)
        {
            if (source is ExternStruct ea && arg is ExternStruct eb) return ea.Struct == eb.Struct;
            if (source is StructBody sa && arg is StructBody sb) return sa == sb;
            if (arg is NumericStruct num) return num.Types.Or(x => TypeEquals(source, x));
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

        public static TypeMapper? GetTypemapperOrNull(Dictionary<GenericsMapper, TypeMapper> sp, GenericsMapper g)
        {
            foreach (var kv in sp)
            {
                if (kv.Key.Keys.And(x => g.ContainsKey(x) && kv.Key[x] == g[x])) return kv.Value;
            }
            return null;
        }

        public static TypeMapper GetTypemapper(Dictionary<GenericsMapper, TypeMapper> sp, GenericsMapper g) => GetTypemapperOrNull(sp, g)!;
    }
}
