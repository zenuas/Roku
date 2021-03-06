﻿using Extensions;
using Roku.Declare;
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

        public static IEnumerable<StructBody> AllStructBodies(List<SourceCodeBody> srcs) => srcs.Map(AllStructBodies).Flatten();

        public static IEnumerable<StructBody> AllStructBodies(INamespaceBody src) => src.Structs.By<StructBody>();

        public static IEnumerable<ExternStruct> AllExternStructs(RootNamespace root) => AllStructs<ExternStruct>(root);

        public static IEnumerable<T> AllStructs<T>(INamespaceBody src) where T : IStructBody => src.Structs.By<T>();

        public static IEnumerable<IFunctionBody> AllFunctionBodies(List<SourceCodeBody> srcs) => srcs.Map(AllFunctionBodies).Flatten();

        public static IEnumerable<IFunctionBody> AllFunctionBodies(INamespaceBody src) => src.Functions.By<IFunctionBody>();

        public static IEnumerable<ExternFunction> AllExternFunctions(List<INamespace> srcs) => srcs.Map(AllFunctions<ExternFunction>).Flatten();

        public static IEnumerable<EmbeddedFunction> AllEmbeddedFunctions(List<INamespace> srcs) => srcs.Map(AllFunctions<EmbeddedFunction>).Flatten();

        public static IEnumerable<T> AllFunctions<T>(INamespace src) where T : IFunctionName => src is INamespaceBody ns ? ns.Functions.By<T>() : new List<T>();

        public static FunctionSpecialization? FindFunctionOrNull(INamespace ns, string name, List<IStructBody?> args, bool find_use = true)
        {
            if (ns is INamespaceBody nsb)
            {
                foreach (var x in nsb.Functions.Where(x => x.Name == name))
                {
                    var v = FunctionArgumentsEquals(ns, x, args);
                    if (v.Exists)
                    {
                        if (x is ISpecialization sp) AppendSpecialization(sp, v.GenericsMapper);
                        return new FunctionSpecialization(x, v.GenericsMapper);
                    }
                }
            }

            if (ns is ILexicalScope lex) return FindFunctionOrNull(lex.Namespace, name, args, find_use);

            if (find_use && ns is IUse body)
            {
                foreach (var use in body.Uses)
                {
                    if (FindFunctionOrNull(use, name, args, false) is { } x) return x;
                }
            }
            if (ns is StructSpecialization tsp && tsp.Body is ExternStruct sx)
            {
                foreach (var m in sx.Struct.GetMethods().Where(x => MatchMethodName(x, name) && x.GetParameters().Length + (x.IsStatic ? 0 : 1) == args.Count))
                {
                    var g = new GenericsMapper();

                    //var args_ti = m.GetParameters().Map(x => x.ParameterType.GetTypeInfo()).ToList();
                    //if (!m.IsStatic) args_ti.Insert(0, sx.Struct);

                    sx.Struct.GetGenericArguments().Each(x => g.Add(new TypeGenericsParameter(x.Name), tsp.GenericsMapper.GetValue(x.Name)));
                    //m.GetGenericArguments().Each(x => g.Add(new TypeValue(x.Name) { Types = Types.Generics }, null));

                    //ToDo: List patch
                    var ti = m.DeclaringType!;
                    var asmx = ti.Assembly;
                    if (ti == typeof(List<>).GetTypeInfo()) asmx = Assembly.Load("System.Collections");

                    return new FunctionSpecialization(new ExternFunction(name, m, asmx), g);
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

        public static (bool Exists, GenericsMapper GenericsMapper) FunctionArgumentsEquals(INamespace ns, IFunctionName source, List<IStructBody?> args)
        {
            if (source is FunctionTypeBody ftb)
            {
                return (false, new GenericsMapper());
            }
            else
            {
                var gens = ApplyArgumentsToGenericsParameter(ns, source, args);
                var fargs = GetArgumentsType(ns, source, gens);
                return (fargs.Count == args.Count && fargs.Zip(args).And(x => TypeEquals(x.First, x.Second)), gens);
            }
        }

        public static List<IStructBody?> GetArgumentsType(INamespace ns, IFunctionName body, GenericsMapper gens) => FunctionToArgumentsType(body).Map(x => GetStructType(ns, x, gens)).ToList();

        public static IStructBody? GetStructType(INamespace ns, ITypeDefinition t, GenericsMapper gens)
        {
            switch (t)
            {
                case TypeGenericsParameter gen:
                    return gens[gen]!;

                case TypeSpecialization g:
                    return FindStructOrNull(ns, new string[] { g.Name }, g.Generics.Map(x => GetStructType(ns, x, gens)!).ToList());

                case TypeValue tv:
                    return LoadStruct(ns, tv.Name);

                case TypeInfoValue ti:
                    return LoadType(GetRootNamespace(ns), ti.Type);

                case TypeEnum te:
                    return LoadEnumStruct(ns, te);

                case TypeFunction tf:
                    return LoadFunctionType(ns, tf, gens);
            }
            throw new Exception();
        }

        public static IStructBody? GetStructType(INamespace ns, ITypeDefinition t, TypeMapper mapper) => GetStructType(ns, t, TypeMapperToGenericsMapper(mapper));

        public static IEnumerable<ITypeDefinition> FunctionToArgumentsType(IFunctionName body)
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
            else if (body is FunctionTypeBody)
            {
                throw new Exception();
            }
            throw new Exception();
        }

        public static IEnumerable<IEvaluable> FunctionToArgumentsName(IFunctionName body)
        {
            if (body is FunctionBody fb)
            {
                return fb.Arguments.Map(x => x.Name);
            }
            else if (body is EmbeddedFunction ef)
            {
                return ef.Arguments;
            }
            else if (body is ExternFunction xf)
            {
                throw new Exception();
            }
            else if (body is FunctionTypeBody)
            {
                throw new Exception();
            }
            throw new Exception();
        }

        public static GenericsMapper ApplyArgumentsToGenericsParameter(INamespace ns, IFunctionName body, List<IStructBody?> args)
        {
            var param = FunctionToArgumentsType(body).ToList();
            var gens = new GenericsMapper();
            if (body is ISpecialization sp) sp.Generics.Each(x => gens[x] = null);

            Action<ITypeDefinition, IStructBody?> match = (p, arg) =>
            {
                if (p is TypeGenericsParameter v) gens[v] = arg;
            };
            param.Each((x, i) => match(x, args.Count > i ? args[i] : null));

            if (body is IConstraints constr && constr.Constraints.Count > 0)
            {
                while (gens.Or(x => x.Value is null))
                {
                    constr.Constraints.Each(x =>
                    {
                        if (FindClassOrNull(ns, x.Class.Name, x.Generics) is { } class_body)
                        {
                            var class_gens = new GenericsMapper();
                            class_body.Generics.Each((g, i) => class_gens[g] = gens[x.Generics[i]]);
                            if (ApplyClassToGenericsParameter(ns, class_body, class_gens))
                            {
                                class_body.Generics.Each((g, i) => gens[x.Generics[i]] = class_gens[g]);
                            }
                        }
                    });
                }
            }

            if (body is ISpecialization sp2 &&
                Lookup.GetGenericsTypeMapperOrNull(sp2.SpecializationMapper, gens) is { } gm)
            {
                return gm.GenericsMapper;
            }

            return gens;
        }

        public static ClassBody? FindClassOrNull(INamespace ns, string name, List<TypeGenericsParameter> gens)
        {
            if (ns is INamespaceBody nsb)
            {
                if (nsb.Classes.FindFirstOrNull(x => x.Name == name && x.Generics.Count == gens.Count) is { } c) return c;
            }
            return null;
        }

        public static bool ApplyClassToGenericsParameter(INamespace ns, ClassBody class_body, GenericsMapper g)
        {
            Func<IStructBody?, IStructBody?, bool> feedback = (left, right) =>
            {
                if (right is null) return false;

                if (left is null) return true;
                if (left is NumericStruct num && num.Types.Contains(right)) return true;

                return false;
            };

            var resolved = false;
            class_body.Functions.By<FunctionBody>().Each(f =>
            {
                var args = f.Arguments.Map(x => g.ContainsKey(x.Type) ? g[x.Type] : LoadStruct(ns, x.Name.Name)).ToList();
                var caller = FindFunctionOrNull(ns, f.Name, args);
                if (caller is { })
                {
                    var fm = Typing.CreateFunctionMapper(ns, caller);
                    var fargs = FunctionToArgumentsName(caller.Body).ToList();
                    f.Arguments.Each((x, i) =>
                    {
                        if (g.ContainsKey(x.Type))
                        {
                            if (feedback(g[x.Type], fm.TypeMapper[fargs[i]].Struct))
                            {
                                g[x.Type] = fm.TypeMapper[fargs[i]].Struct;
                                resolved = true;
                            }
                        }
                    });
                    if (f.Return is { } && g.ContainsKey(f.Return) && caller.Body is IFunctionReturn r && r.Return is { } ret)
                    {
                        if (feedback(g[f.Return], fm.TypeMapper[ret].Struct))
                        {
                            g[f.Return] = fm.TypeMapper[ret].Struct;
                            resolved = true;
                        }
                    }
                    resolved = true;
                }
            });
            return resolved;
        }

        public static bool TypeEquals(IStructBody? source, IStructBody? arg)
        {
            if (source is ExternStruct ea && arg is ExternStruct eb) return ea.Struct == eb.Struct;
            if (source is StructBody sa && arg is StructBody sb) return sa == sb;
            if (source is StructSpecialization ssa && arg is StructSpecialization ssb) return TypeEquals(ssa.Body, ssb.Body) && ssa.GenericsMapper.And(x => TypeEquals(x.Value, ssb.GenericsMapper[x.Key]));
            if (arg is NumericStruct num) return num.Types.Or(x => TypeEquals(source, x));
            if (source is NumericStruct num2) return num2.Types.Or(x => TypeEquals(x, arg));
            if (source is FunctionTypeBody fta && arg is AnonymousFunctionBody afb) return TypeFunctionEquals(fta, afb);
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

        public static bool TypeFunctionEquals(FunctionTypeBody ft, AnonymousFunctionBody af)
        {
            return ft.Name == af.ToString(!(af.IsImplicit && ft.Return is null));
        }

        public static void AppendSpecialization(ISpecialization sp, GenericsMapper g)
        {
            if (Lookup.GetGenericsTypeMapperOrNull(sp.SpecializationMapper, g).HasValue) return;
            var mapper = sp.SpecializationMapper[g] = new TypeMapper();
            g.Each(kv => mapper[kv.Key] = Typing.CreateVariableDetail(kv.Key.Name, kv.Value, VariableType.TypeParameter));
        }

        public static StructSpecialization? FindStructOrNull(INamespace ns, string[] name, List<IStructBody> args)
        {
            if (ns is INamespaceBody nsb)
            {
                foreach (var x in nsb.Structs.Where(x => TypeNameEquals(x, name)))
                {
                    if (x is ISpecialization g)
                    {
                        if (g.Generics.Count != args.Count) continue;
                        var gens = new GenericsMapper();
                        g.Generics.Each((x, i) => gens[x] = args[i]);
                        AppendSpecialization(g, gens);
                        return new StructSpecialization(x, gens);
                    }
                    else
                    {
                        return new StructSpecialization(x, new GenericsMapper());
                    }
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
                        return new StructSpecialization(t, gens);
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

        public static FunctionTypeBody LoadFunctionType(INamespace ns, TypeFunction tf, GenericsMapper gens)
        {
            var func = new FunctionTypeBody();
            tf.Arguments.Each(x => func.Arguments.Add(GetStructType(ns, x, gens)!));
            if (tf.Return is { }) func.Return = GetStructType(ns, tf.Return, gens);
            return func;
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

        public static ExternFunction LoadFunction(RootNamespace root, INamespaceBody ns, string alias, MethodInfo mi)
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

        public static INamespaceBody GetTopLevelNamespace(INamespace ns) =>
            ns is RootNamespace root ? root
            : ns is SourceCodeBody src ? src
            : ns is ILexicalScope lex ? GetTopLevelNamespace(lex.Namespace)
            : throw new Exception();

        public static IStructBody? LoadTypeWithoutVoid(RootNamespace root, Type t, GenericsMapper g) =>
            t == typeof(void) ? null
            : t.IsGenericParameter ? g.GetValue(t.Name)
            : LoadType(root, t.GetTypeInfo());

        public static (GenericsMapper GenericsMapper, TypeMapper TypeMapper)? GetGenericsTypeMapperOrNull(Dictionary<GenericsMapper, TypeMapper> sp, GenericsMapper g)
        {
            foreach (var kv in sp)
            {
                if (kv.Key.Keys.And(x => g.ContainsKey(x) && (kv.Key[x] == g[x] || g[x] is null))) return (kv.Key, kv.Value);
            }
            return null;
        }

        public static TypeMapper? GetTypemapperOrNull(Dictionary<GenericsMapper, TypeMapper> sp, GenericsMapper g) => GetGenericsTypeMapperOrNull(sp, g)?.TypeMapper;

        public static TypeMapper GetTypemapper(Dictionary<GenericsMapper, TypeMapper> sp, GenericsMapper g) => GetTypemapperOrNull(sp, g)!;

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
