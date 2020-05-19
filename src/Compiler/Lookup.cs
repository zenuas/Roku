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

        public static IEnumerable<StructBody> AllStructBodies(List<SourceCodeBody> srcs) => srcs.Map(AllStructs<StructBody>).Flatten();

        public static IEnumerable<ExternStruct> AllExternStructs(RootNamespace root) => AllStructs<ExternStruct>(root);

        public static IEnumerable<T> AllStructs<T>(INamespace src) where T : IStructBody => src.Structs.By<T>();

        public static IEnumerable<FunctionBody> AllFunctionBodies(List<SourceCodeBody> srcs) => srcs.Map(AllFunctionBodies).Flatten();

        public static IEnumerable<FunctionBody> AllFunctionBodies(SourceCodeBody src) => src.Functions.By<FunctionBody>();

        public static IEnumerable<ExternFunction> AllExternFunctions(List<INamespace> srcs) => srcs.Map(AllFunctions<ExternFunction>).Flatten();

        public static IEnumerable<EmbeddedFunction> AllEmbeddedFunctions(List<INamespace> srcs) => srcs.Map(AllFunctions<EmbeddedFunction>).Flatten();

        public static IEnumerable<T> AllFunctions<T>(INamespace src) where T : IFunctionBody => src.Functions.By<T>();

        public static FunctionCaller? FindFunctionOrNull(INamespace ns, string name, List<IStructBody?> args)
        {
            foreach (var x in ns.Functions.Where(x => x.Name == name))
            {
                var v = FunctionArgumentsEquals(ns, x, args);
                if (v.Exists) return new FunctionCaller(x, v.GenericsMapper);
            }

            if (ns is SourceCodeBody body)
            {
                foreach (var use in body.Uses)
                {
                    if (FindFunctionOrNull(use, name, args) is { } x) return x;
                }
            }
            return null;
        }

        public static (bool Exists, GenericsMapper GenericsMapper) FunctionArgumentsEquals(INamespace ns, IFunctionBody source, List<IStructBody?> args)
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

        public static IStructBody? GetStructType(INamespace ns, ITypeDefinition t, TypeMapper mapper)
        {
            if (t is TypeValue gen && gen.Types == Types.Generics)
            {
                //return mapper[gen]!;
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

        public static TypeSpecialization? FindStructOrNull(INamespace ns, string name, List<IStructBody> args)
        {
            foreach (var x in ns.Structs.Where(x => x.Name == name))
            {
                if (x is ISpecialization g)
                {
                    if (g.Generics.Count != args.Count) continue;
                    var gens = new GenericsMapper();
                    g.Generics.Each((x, i) => gens[x] = args[i]);
                    return new TypeSpecialization(x, gens);
                }
                else
                {
                    return new TypeSpecialization(x, new GenericsMapper());
                }
            }

            if (ns is SourceCodeBody body)
            {
                foreach (var use in body.Uses)
                {
                    if (FindStructOrNull(use, name, args) is { } x) return x;
                }
            }
            return null;
        }

        public static IStructBody LoadStruct(INamespace ns, string name) => FindStructOrNull(ns, name, new List<IStructBody>())?.Body ?? throw new Exception();

        public static IEnumerable<LabelCode> AllLabels(List<IOperand> ops) => ops.By<LabelCode>().Unique();

        public static ExternStruct LoadType(RootNamespace root, Type t) => LoadType(root, t.Name, t);

        public static ExternStruct LoadType(RootNamespace root, string name, Type t) => LoadType(root, name, t.GetTypeInfo());

        public static ExternStruct LoadType(RootNamespace root, TypeInfo ti) => LoadType(root, ti.Name, ti);

        public static ExternStruct LoadType(RootNamespace root, string name, TypeInfo ti)
        {
            var st = root.Structs.Where(x => x is ExternStruct sx && sx.Struct == ti).FirstOrNull();
            if (st is { }) return st.Cast<ExternStruct>();

            Assembly? asmx = null;
            foreach (var asm in root.Assemblies)
            {
                var x = GetAssemblyType(asm).FindFirstOrNull(x => x.GetTypeInfo() == ti);
                if (x is { })
                {
                    asmx = asm;
                    break;
                }
            }
            if (asmx is null) root.Assemblies.Add(asmx = ti.Assembly);

            var t = new ExternStruct(name, ti, asmx);
            root.Structs.Add(t);
            return t;
        }

        public static IEnumerable<Type> GetAssemblyType(Assembly asm)
        {
            foreach (var t in asm.GetTypes())
            {
                yield return t;
            }
            foreach (var t in asm.GetReferencedAssemblies().Map(Assembly.Load).Map(GetAssemblyType).Flatten())
            {
                yield return t;
            }
        }

        public static ExternFunction LoadFunction(RootNamespace root, string alias, MethodInfo mi)
        {
            var f = new ExternFunction(alias, mi, LoadType(root, mi.DeclaringType!).Assembly);
            root.Functions.Add(f);
            return f;
        }

        public static RootNamespace GetRootNamespace(INamespace ns) => ns is RootNamespace root ? root : GetRootNamespace(ns.Parent!);

        public static ExternStruct? LoadTypeWithoutVoid(RootNamespace root, Type t) => LoadTypeWithoutVoid(root, t.GetTypeInfo());

        public static ExternStruct? LoadTypeWithoutVoid(RootNamespace root, TypeInfo ti) => ti == typeof(void) ? null : LoadType(root, ti.Name, ti);

        public static (GenericsMapper GenericsMapper, TypeMapper TypeMapper)? GetGenericsTypeMapperOrNull(Dictionary<GenericsMapper, TypeMapper> sp, GenericsMapper g)
        {
            foreach (var kv in sp)
            {
                if (kv.Key.Keys.And(x => g.ContainsKey(x) && kv.Key[x] == g[x])) return (kv.Key, kv.Value);
            }
            return null;
        }

        public static TypeMapper? GetTypemapperOrNull(Dictionary<GenericsMapper, TypeMapper> sp, GenericsMapper g) => GetGenericsTypeMapperOrNull(sp, g)?.TypeMapper;

        public static TypeMapper GetTypemapper(Dictionary<GenericsMapper, TypeMapper> sp, GenericsMapper g) => GetTypemapperOrNull(sp, g)!;

        public static bool IsValueType(IStructBody? t) => t is ExternStruct sx && sx.Struct.IsValueType;
    }
}
