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
                if (source is IUse body) xs.AddRange(body.Uses.Map(xs => use_load(xs)).Flatten());
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

        public static FunctionCaller? FindFunctionOrNull(INamespace ns, string name, List<IStructBody?> args, bool find_use = true)
        {
            foreach (var x in ns.Functions.Where(x => x.Name == name))
            {
                var v = FunctionArgumentsEquals(ns, x, args);
                if (v.Exists) return new FunctionCaller(x, v.GenericsMapper);
            }

            if (ns is ILexicalScope lex) return FindFunctionOrNull(lex.Namespace, name, args, find_use);

            if (find_use && ns is IUse body)
            {
                foreach (var use in body.Uses)
                {
                    if (FindFunctionOrNull(use, name, args, false) is { } x) return x;
                }
            }
            if (ns is TypeSpecialization sp && sp.Body is ExternStruct sx)
            {
                foreach (var m in sx.Struct.GetMethods().Where(x => MatchMethodName(x, name) && x.GetParameters().Length + (x.IsStatic ? 0 : 1) == args.Count))
                {
                    var g = new GenericsMapper();

                    //var args_ti = m.GetParameters().Map(x => x.ParameterType.GetTypeInfo()).ToList();
                    //if (!m.IsStatic) args_ti.Insert(0, sx.Struct);

                    sx.Struct.GetGenericArguments().Each(x => g.Add(new TypeGenericsParameter(x.Name), sp.GenericsMapper.GetValue(x.Name)));
                    //m.GetGenericArguments().Each(x => g.Add(new TypeValue(x.Name) { Types = Types.Generics }, null));

                    //ToDo: List patch
                    var ti = m.DeclaringType!;
                    var asmx = ti.Assembly;
                    if (ti == typeof(List<>).GetTypeInfo()) asmx = Assembly.Load("System.Collections");

                    return new FunctionCaller(new ExternFunction(name, m, asmx), g);
                }
            }
            return null;
        }

        public static bool MatchMethodName(MethodInfo mi, string name)
        {
            if (!mi.IsSpecialName) return mi.Name == name;
            switch (name)
            {
                case "[]": return mi.Name.In("get_Item", "set_Item");
                case "+": return mi.Name == "op_Addition";
                case "-": return mi.Name == "op_Subtraction";
                case "==": return mi.Name == "op_Equality";
                case "!=": return mi.Name == "op_Inequality";
                case "<": return mi.Name == "op_LessThan";
                case "<=": return mi.Name == "op_LessThanOrEqual";
                case ">": return mi.Name == "op_GreaterThan";
                case ">=": return mi.Name == "op_GreaterThanOrEqual";
                default: return !new string[] { "get_", "set_", "add_", "remove_" }.Where(x => mi.Name.StartsWith(x) && mi.Name.Substring(x.Length) == name).IsNull();
            }
            throw new Exception();
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
            if (t is TypeGenericsParameter gen)
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
            else if (t is TypeEnum te)
            {
                return LoadEnumStruct(ns, te);
            }
            throw new Exception();
        }

        public static IStructBody? GetStructType(INamespace ns, ITypeDefinition t, TypeMapper mapper)
        {
            if (t is TypeGenericsParameter gen)
            {
                return mapper[gen]!.Struct;
            }
            else if (t is TypeValue tv)
            {
                return LoadStruct(ns, tv.Name);
            }
            else if (t is TypeInfoValue ti)
            {
                return LoadType(GetRootNamespace(ns), ti.Type);
            }
            else if (t is TypeEnum te)
            {
                return LoadEnumStruct(ns, te);
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

        public static IEnumerable<TypeGenericsParameter> ExtractArgumentsTypeToGenericsParameter(IEnumerable<ITypeDefinition> types) => types.By<TypeGenericsParameter>().Unique();

        public static GenericsMapper ApplyArgumentsToGenericsParameter(IFunctionBody body, List<IStructBody?> args)
        {
            var param = FunctionToArgumentsType(body).ToList();
            var gens = new GenericsMapper();
            ExtractArgumentsTypeToGenericsParameter(param).Each(x => gens[x] = null);

            Action<ITypeDefinition, IStructBody?> match = (p, arg) =>
            {
                if (p is TypeGenericsParameter v) gens[v] = arg;
            };
            param.Each((x, i) => match(x, args.Count > i ? args[i] : null));

            return gens;
        }

        public static bool TypeEquals(IStructBody? source, IStructBody? arg)
        {
            if (source is ExternStruct ea && arg is ExternStruct eb) return ea.Struct == eb.Struct;
            if (source is StructBody sa && arg is StructBody sb) return sa == sb;
            if (arg is NumericStruct num) return num.Types.Or(x => TypeEquals(source, x));
            if (source is NumericStruct num2) return num2.Types.Or(x => TypeEquals(x, arg));
            return false;
        }

        public static bool TypeNameEquals(TypeInfo ti, string[] name)
        {
            var full_name = name.Join(".");
            if (ti.FullName == full_name) return true;
            var g = ti.FullName!.IndexOf("`");
            return g >= 0 && ti.FullName.Substring(0, g) == full_name;
        }

        public static bool TypeNameEquals(IStructBody body, string[] name)
        {
            switch (body)
            {
                case StructBody x:
                    return name.Length == 1 && x.Name == name.First();

                case ExternStruct x:
                    return (name.Length == 1 && x.Name == name.First()) || TypeNameEquals(x.Struct, name);
            }
            return false;
        }

        public static TypeSpecialization? FindStructOrNull(INamespace ns, string[] name, List<IStructBody> args)
        {
            foreach (var x in ns.Structs.Where(x => TypeNameEquals(x, name)))
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

            if (ns is ILexicalScope lex) return FindStructOrNull(lex.Namespace, name, args);

            if (ns is RootNamespace root)
            {
                foreach (var asm in root.Assemblies)
                {
                    foreach (var ti in GetAssemblyType(asm).Map(x => x.GetTypeInfo()).Where(x => TypeNameEquals(x, name)))
                    {
                        if (ti.GetGenericArguments().Length != args.Count) continue;

                        //ToDo: List patch
                        var asmx = asm;
                        if (ti == typeof(List<>).GetTypeInfo()) asmx = Assembly.Load("System.Collections");

                        var t = CreateExternStruct(root, ti, asmx);

                        var gens = new GenericsMapper();
                        t.Generics.Each((x, i) => gens[x] = args[i]);
                        return new TypeSpecialization(t, gens);
                    }
                }
            }
            else if (ns is IUse body)
            {
                foreach (var use in body.Uses)
                {
                    if (FindStructOrNull(use, name, args) is { } x) return x;
                }
            }
            return null;
        }

        public static IStructBody LoadStruct(INamespace ns, string[] name) => FindStructOrNull(ns, name, new List<IStructBody>())?.Body ?? throw new Exception();

        public static IStructBody LoadStruct(INamespace ns, string name) => LoadStruct(ns, new string[] { name });

        public static IEnumerable<LabelCode> AllLabels(List<IOperand> ops) => ops.By<LabelCode>().Unique();

        public static ExternStruct LoadType(RootNamespace root, Type t) => LoadType(root, t.GetTypeInfo());

        public static ExternStruct LoadType(RootNamespace root, TypeInfo ti)
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

            return CreateExternStruct(root, ti, asmx);
        }

        public static IStructBody LoadEnumStruct(INamespace ns, TypeEnum te)
        {
            var e = new EnumStructBody(ns);
            foreach (var td in te.Enums)
            {
                e.Enums.Add(GetStructType(ns, td, new TypeMapper())!);
            }
            return e;
        }

        public static ExternStruct CreateExternStruct(RootNamespace root, TypeInfo ti, Assembly asm)
        {
            var t = new ExternStruct(ti, asm);
            root.Structs.Add(t);

            ti.GenericTypeParameters.Each(x => t.Generics.Add(new TypeGenericsParameter(x.Name)));
            ti.GetMethods().Each(x => LoadFunction(root, t, x.Name, x));

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

        public static ExternFunction LoadFunction(RootNamespace root, string alias, MethodInfo mi) => LoadFunction(root, root, alias, mi);

        public static ExternFunction LoadFunction(RootNamespace root, INamespace ns, string alias, MethodInfo mi)
        {
            var f = new ExternFunction(alias, mi, LoadType(root, mi.DeclaringType!).Assembly);
            ns.Functions.Add(f);
            return f;
        }

        public static RootNamespace GetRootNamespace(INamespace ns) =>
            ns is RootNamespace root ? root
            : ns is ILexicalScope lex ? GetRootNamespace(lex.Namespace)
            : ns is IUse use ? use.Uses.By<RootNamespace>().First()
            : throw new Exception();

        public static IStructBody? LoadTypeWithoutVoid(RootNamespace root, Type t, GenericsMapper g) =>
            t == typeof(void) ? null
            : t.IsGenericParameter ? g.GetValue(t.Name)
            : LoadType(root, t.GetTypeInfo());

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

        public static TypeMapper GenericsMapperToTypeMapper(GenericsMapper g)
        {
            var mapper = new TypeMapper();
            g.Each(kv => mapper[kv.Key] = Typing.CreateVariableDetail(kv.Key.Name, kv.Value, VariableType.TypeParameter));
            return mapper;
        }

        public static GenericsMapper TypeMapperToGenericsMapper(TypeMapper tm)
        {
            var g = new GenericsMapper();
            tm.Where(x => x.Value.Type == VariableType.TypeParameter).Each(x => g[x.Key.Cast<ITypeDefinition>()] = x.Value.Struct);
            return g;
        }

        public static bool IsValueType(IStructBody? body) =>
            (body is TypeInfoValue t && t.Type.IsValueType) ||
            (body is ExternStruct e && e.Struct.IsValueType) ||
            (body is NumericStruct);
    }
}
