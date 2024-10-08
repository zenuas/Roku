﻿using Mina.Extension;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Roku.Compiler;

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
                _ = readed.Add(source);
            }
            xs.AddRange(source.Uses.OfType<SourceCodeBody>().Select(xs => use_load(xs)).Flatten());
            return xs;
        }
        return use_load(src);
    }

    public static List<IManaged> AllNamespaces(SourceCodeBody src)
    {
        var readed = new HashSet<IManaged>();

        List<IManaged> use_load(IManaged source)
        {
            var xs = new List<IManaged>();
            if (!readed.Contains(source))
            {
                xs.Add(source);
                _ = readed.Add(source);
            }
            if (source is IUse body) xs.AddRange(body.Uses.Select(xs => use_load(xs)).Flatten());
            return xs;
        }
        return use_load(src);
    }

    public static IEnumerable<StructBody> AllStructBodies(List<SourceCodeBody> srcs) => srcs.Select(AllStructBodies).Flatten();

    public static IEnumerable<StructBody> AllStructBodies(INamespace src) => src.Structs.OfType<StructBody>();

    public static IEnumerable<ExternStruct> AllExternStructs(RootNamespace root) => AllStructs<ExternStruct>(root);

    public static IEnumerable<T> AllStructs<T>(INamespace src) where T : IStructBody => src.Structs.OfType<T>();

    public static IEnumerable<IFunctionBody> AllFunctionBodies(List<SourceCodeBody> srcs) => srcs.Select(AllFunctionBodies).Flatten();

    public static IEnumerable<IFunctionBody> AllFunctionBodies(INamespace src) => src.Functions.OfType<IFunctionBody>();

    public static IEnumerable<ExternFunction> AllExternFunctions(List<IManaged> srcs) => srcs.Select(AllFunctions<ExternFunction>).Flatten();

    public static IEnumerable<EmbeddedFunction> AllEmbeddedFunctions(List<IManaged> srcs) => srcs.Select(AllFunctions<EmbeddedFunction>).Flatten();

    public static IEnumerable<T> AllFunctions<T>(IManaged src) where T : IFunctionName => src is INamespace ns ? ns.Functions.OfType<T>() : [];

    public static IEnumerable<InstanceBody> AllInstanceBodies(List<SourceCodeBody> srcs) => srcs.Select(AllInstanceBodies).Flatten();

    public static IEnumerable<InstanceBody> AllInstanceBodies(INamespace src) => src.Instances;

    public static FunctionSpecialization? IfFunctionArgumentsEquals_ThenAppendSpecialization(IManaged ns, IFunctionName fn, List<IStructBody?> args)
    {
        var v = FunctionArgumentsEquals(ns, fn, args);
        if (v.Exists)
        {
            if (fn is ISpecialization sp) _ = AppendSpecialization(sp, v.GenericsMapper);
            return new() { Body = fn, GenericsMapper = v.GenericsMapper };
        }
        return null;
    }

    public static bool IsReferable(ILexicalScope current, ILexicalScope lex)
    {
        if (current == lex) return true;
        if (current.Parent is { }) return IsReferable(current.Parent, lex);
        return lex.Namespace is RootNamespace;
    }

    public static FunctionSpecialization? FindFunctionOrNull(IManaged ns, ILexicalScope? current, string name, List<IStructBody?> args, bool find_use = true)
    {
        if (ns is INamespace nsb)
        {
            foreach (var x in nsb.Functions.Where(x => x.Name == name))
            {
                if (current is { } && x is ILexicalScope lex && lex.Parent is { } && !IsReferable(current, lex.Parent)) continue;
                if (IfFunctionArgumentsEquals_ThenAppendSpecialization(ns, x, args) is { } p) return p;
            }
        }

        if (ns is IAttachedNamespace ans) return FindFunctionOrNull(ans.Namespace, current, name, args, find_use);

        if (find_use && ns is IUse body)
        {
            foreach (var use in body.Uses)
            {
                if (FindFunctionOrNull(use, current, name, args, false) is { } x) return x;
            }
        }
        if (ns is StructSpecialization tsp && tsp.Body is ExternStruct sx)
        {
            foreach (var m in sx.Struct.GetMethods().Where(x => MatchMethodName(x, name) && x.GetParameters().Length + (x.IsStatic ? 0 : 1) == args.Count))
            {
                var g = new GenericsMapper();

                //var args_ti = m.GetParameters().Map(x => x.ParameterType.GetTypeInfo()).ToList();
                //if (!m.IsStatic) args_ti.Insert(0, sx.Struct);

                sx.Struct.GetGenericArguments().Each(x => g.Add(new TypeGenericsParameter() { Name = x.Name }, tsp.GenericsMapper.GetValue(x.Name)));
                //m.GetGenericArguments().Each(x => g.Add(new TypeValue(x.Name) { Types = Types.Generics }, null));

                //ToDo: List patch
                var ti = m.DeclaringType!;
                var asmx = ti.Assembly;
                if (ti == typeof(List<>).GetTypeInfo()) asmx = Assembly.Load("System.Collections");

                return new() { Body = new ExternFunction { Name = name, Function = m, Assembly = asmx }, GenericsMapper = g };
            }
        }
        if (ns is ExternStruct sx2)
        {
            foreach (var m in sx2.Struct.GetMethods().Where(x => MatchMethodName(x, name) && x.GetParameters().Length + (x.IsStatic ? 0 : 1) == args.Count))
            {
                var g = new GenericsMapper();

                //ToDo: String patch
                var ti = m.DeclaringType!;
                var asmx = ti.Assembly;
                if (ti == typeof(string).GetTypeInfo()) asmx = Assembly.Load("System.Runtime");

                return new() { Body = new ExternFunction { Name = name, Function = m, Assembly = asmx }, GenericsMapper = g };
            }
        }
        return null;
    }

    public static bool MatchMethodName(MethodInfo mi, string name)
    {
        if (!mi.IsSpecialName) return mi.Name == name;
        return name switch
        {
            "[]" => mi.Name.In("get_Item", "set_Item"),
            "+" => mi.Name == "op_Addition",
            "-" => mi.Name == "op_Subtraction",
            "==" => mi.Name == "op_Equality",
            "!=" => mi.Name == "op_Inequality",
            "<" => mi.Name == "op_LessThan",
            "<=" => mi.Name == "op_LessThanOrEqual",
            ">" => mi.Name == "op_GreaterThan",
            ">=" => mi.Name == "op_GreaterThanOrEqual",
            _ => !new string[] { "get_", "set_", "add_", "remove_" }.Where(x => mi.Name.StartsWith(x) && mi.Name[x.Length..] == name).IsEmpty(),
        };
    }

    public static (bool Exists, GenericsMapper GenericsMapper) FunctionArgumentsEquals(IManaged ns, IFunctionName source, List<IStructBody?> args)
    {
        if (source is FunctionTypeBody ftb)
        {
            return (false, new GenericsMapper());
        }
        else
        {
            var gens = ApplyArgumentsToGenericsParameter(ns, source, args);
            var fargs = GetArgumentsType(ns, source, gens);
            return (fargs.Count == args.Count && fargs.Zip(args).All(x => TypeEquals(x.First, x.Second)), gens);
        }
    }

    public static List<IStructBody?> GetArgumentsType(IManaged ns, IFunctionName body, GenericsMapper gens) => FunctionToArgumentsType(body).Select(x => GetStructType(ns, x, gens)).ToList();

    public static string[] GetTypeNames(IEvaluable e) =>
        e is TypeValue tv ? [.. tv.Namespace, tv.Name]
        : e is VariableValue v ? new string[] { v.Name }
        : throw new();

    public static IStructBody? GetStructType(IManaged ns, ITypeDefinition t, GenericsMapper gens)
    {
        switch (t)
        {
            case TypeGenericsParameter gen:
                return gens[gen]!;

            case TypeSpecialization g:
                {
                    var gx = g.Generics.Select(x => GetStructType(ns, x, gens));
                    if (gx.Contains(x => x is null)) return null;
                    return FindStructOrNull(ns, GetTypeNames(g.Type), gx.Select(x => x!).ToList());
                }

            case TypeValue tv:
                return LoadStruct(ns, tv.Name);

            case TypeInfoValue ti:
                return LoadType(GetRootNamespace(ns), ti.Type);

            case TypeEnum te:
                return LoadEnumStruct(ns, te);

            case TypeFunction tf:
                return LoadFunctionType(ns, tf, gens);
        }
        throw new();
    }

    public static IStructBody? GetStructType(IManaged ns, ITypeDefinition t, TypeMapper mapper) => GetStructType(ns, t, TypeMapperToGenericsMapper(mapper));

    public static IEnumerable<ITypeDefinition> FunctionToArgumentsType(IFunctionName body)
    {
        if (body is FunctionBody fb)
        {
            return fb.Arguments.Where(x => !Definition.IsScopeCapturedArgumentName(x.Name.Name)).Select(x => x.Type);
        }
        else if (body is EmbeddedFunction ef)
        {
            return ef.Arguments;
        }
        else if (body is ExternFunction xf)
        {
            return xf.Function.GetParameters().Select(x => new TypeInfoValue { Type = x.ParameterType });
        }
        else if (body is FunctionTypeBody)
        {
            throw new();
        }
        else if (body is AnonymousFunctionBody afb)
        {
            return afb.Arguments.Select(x => x.Type);
        }
        throw new();
    }

    public static IEnumerable<IEvaluable> FunctionToArgumentsName(IFunctionName body)
    {
        if (body is FunctionBody fb)
        {
            return fb.Arguments.Select(x => x.Name);
        }
        else if (body is EmbeddedFunction ef)
        {
            return ef.Arguments;
        }
        else if (body is ExternFunction xf)
        {
            throw new();
        }
        else if (body is FunctionTypeBody)
        {
            throw new();
        }
        throw new();
    }

    public static GenericsMapper ApplyArgumentsToGenericsParameter(IManaged ns, IFunctionName body, List<IStructBody?> args)
    {
        var param = FunctionToArgumentsType(body).ToList();
        var gens = new GenericsMapper();
        if (body is ISpecialization sp) sp.Generics.Each(x => gens[x] = null);

        IStructBody? get_gen(IStructBody? arg, IGenericsMapper gm, int index)
        {
            if (arg is StructSpecialization ssp) return get_gen(ssp.Body, ssp, index);
            if (arg is ISpecialization sp && sp.Generics.Count > index) return gm.GenericsMapper[sp.Generics[index]];
            return null;
        }

        void match(ITypeDefinition p, IStructBody? arg)
        {
            if (p is TypeGenericsParameter v) gens[v] = arg;
            if (p is TypeSpecialization sp && arg is IGenericsMapper gm) sp.Generics.Each((x, i) => match(x, get_gen(arg, gm, i)));
        }
        param.Each((x, i) => match(x, args.Count > i ? args[i] : null));

        if (body is IConstraints constr && constr.Constraints.Count > 0)
        {
            var effected = gens.Any(x => !IsFixedStruct(x.Value));
            while (effected)
            {
                effected = false;

                constr.Constraints.Each(x =>
                {
                    if (FindClassOrNull(ns, x.Class.Name, x.Generics) is { } class_body)
                    {
                        var class_gens = new GenericsMapper();
                        class_body.Generics.Each((g, i) => class_gens[g] = GetStructType(ns, x.Generics[i], gens));
                        if (ApplyClassToGenericsParameter(ns, class_body, class_gens))
                        {
                            //ToDo: List patch
                            if (class_body.Name == "List" && class_body.Generics.Count == 2 &&
                            IsFixedStruct(class_gens[class_body.Generics[1]]) &&
                            class_gens[class_body.Generics[0]] is StructSpecialization ssp &&
                            ssp.Body is ExternStruct es &&
                            es.Struct == typeof(List<>) &&
                            ssp.GenericsMapper[es.Generics[0]] is IndefiniteBody)
                            {
                                var gm = new GenericsMapper
                                {
                                    [es.Generics[0]] = class_gens[class_body.Generics[1]]
                                };
                                class_gens[class_body.Generics[0]] = new StructSpecialization { Body = es, GenericsMapper = gm };
                            }

                            class_body.Generics.Each((g, i) =>
                            {
                                if (GetStructType(ns, x.Generics[i], gens) != class_gens[g])
                                {
                                    effected = true;
                                    gens[x.Generics[i]] = class_gens[g];
                                }
                            });
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

    public static bool IsFixedStruct(IStructBody? sb) =>
        sb is null ? false :
        sb is IndefiniteBody ? false :
        sb is IGenericsMapper gm ? IsFixedGenericsMapper(gm.GenericsMapper) :
        true;

    public static bool IsFixedGenericsMapper(GenericsMapper gm) => gm.All(x => IsFixedStruct(x.Value));

    public static ClassBody? FindClassOrNull(IManaged ns, string name, List<ITypeDefinition> gens)
    {
        if (ns is INamespace nsb)
        {
            if (nsb.Classes.FirstOrDefault(x => x.Name == name && x.Generics.Count == gens.Count) is { } c) return c;
        }
        if (ns is IUse body)
        {
            foreach (var use in body.Uses)
            {
                if (FindClassOrNull(use, name, gens) is { } x) return x;
            }
        }
        return null;
    }

    public static bool ApplyClassToGenericsParameter(IManaged ns, ClassBody class_body, GenericsMapper g)
    {
        Func<IStructBody?, IStructBody?, bool> feedback = (left, right) =>
        {
            if (right is null) return false;

            if (left is null) return true;
            if (left is NumericStruct num && num.Types.Contains(right)) return true;

            return false;
        };

        var resolved = false;
        class_body.Functions.OfType<FunctionBody>().Each(f =>
        {
            var args = f.Arguments.Select(x => g.TryGetValue(x.Type, out var value) ? value : LoadStruct(ns, x.Name.Name)).ToList();
            var caller = FindFunctionOrNull(ns, f, f.Name, args);
            if (caller is { })
            {
                var fm = Typing.CreateFunctionMapper(ns, caller);
                var fargs = FunctionToArgumentsName(caller.Body).ToList();
                f.Arguments.Each((x, i) =>
                {
                    if (g.TryGetValue(x.Type, out var value))
                    {
                        if (feedback(value, fm.TypeMapper[fargs[i]].Struct))
                        {
                            g[x.Type] = fm.TypeMapper[fargs[i]].Struct;
                            resolved = true;
                        }
                    }
                });
                if (f.Return is { } && g.TryGetValue(f.Return, out var value) && caller.Body is IFunctionReturn r && r.Return is { } ret)
                {
                    if (feedback(value, fm.TypeMapper[ret].Struct))
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
        if (source is StructSpecialization ssa && arg is StructSpecialization ssb) return TypeEquals(ssa.Body, ssb.Body) && ssa.GenericsMapper.All(x => TypeEquals(x.Value, ssb.GenericsMapper[x.Key]));
        if (arg is NumericStruct num) return num.Types.Any(x => TypeEquals(source, x));
        if (source is NumericStruct num2) return num2.Types.Any(x => TypeEquals(x, arg));
        if (source is FunctionTypeBody fta && arg is FunctionMapper fm) return TypeFunctionEquals(fta, fm);
        if (source is EnumStructBody e)
        {
            if (arg is EnumStructBody arge) return arge.Enums.All(x => TypeEquals(e, x)); // e >= arge
            return e.Enums.Any(x => TypeEquals(x, arg));
        }
        if (source is NullBody && arg is NullBody) return true;
        if (source is NamespaceBody nsa && arg is NamespaceBody nsb) return nsa == nsb;
        if (source is IndefiniteBody || arg is IndefiniteBody) return true;
        return false;
    }

    public static bool TypeNameEquals(TypeInfo ti, string[] name)
    {
        var full_name = name.Join(".");
        if (ti.FullName == full_name) return true;
        var g = ti.FullName!.IndexOf('`');
        return g >= 0 && ti.FullName[..g] == full_name;
    }

    public static bool TypeNameEquals(IStructBody body, string[] name)
    {
        switch (body)
        {
            case StructBody x:
                return name.Length == 1 && x.Name == name.First();

            case ExternStruct x:
                return (name.Length == 1 && x.Name == name.First()) || TypeNameEquals(x.Struct, name);

            case NullBody x:
                return name.Length == 1 && x.Name == name.First();
        }
        return false;
    }

    public static bool TypeEquals(IStructBody body, ITypeDefinition def)
    {
        if (def is TypeImplicit || def is TypeGenericsParameter) return true;
        return body.Name == def.Name;
    }

    public static bool TypeFunctionEquals(FunctionTypeBody ft, FunctionMapper fm)
    {
        if (fm.Function is IFunctionBody fb) return TypeFunctionEquals(ft, fb);
        return false;
    }

    public static bool TypeFunctionEquals(FunctionTypeBody ft, IFunctionBody fb)
    {
        if (ft.Arguments.Count != fb.Arguments.Count) return false;
        if ((ft.Return is null) != (fb.Return is null) && !(ft.Return is null && fb.Return is TypeImplicit)) return false;
        if (ft.Return is { } fp && fb.Return is { } ap && !TypeEquals(fp, ap)) return false;
        return ft.Arguments.Zip(fb.Arguments).All(arg => TypeEquals(arg.First, arg.Second.Type));
    }

    public static TypeMapper? AppendSpecialization(ISpecialization sp, GenericsMapper g)
    {
        if (Lookup.GetGenericsTypeMapperOrNull(sp.SpecializationMapper, g).HasValue) return null;
        var mapper = sp.SpecializationMapper[g] = [];
        g.Each(kv => mapper[kv.Key] = Typing.CreateVariableDetail(kv.Key.Name, kv.Value, VariableType.TypeParameter));
        return mapper;
    }

    public static IStructBody? FindStructOrNull(IManaged ns, string[] name, List<IStructBody> args)
    {
        if (ns is INamespace nsb)
        {
            foreach (var x in nsb.Structs.Where(x => TypeNameEquals(x, name)))
            {
                if (x is ISpecialization g && g.Generics.Count > 0)
                {
                    if (g.Generics.Count != args.Count) continue;
                    var gens = new GenericsMapper();
                    g.Generics.Each((x, i) => gens[x] = args[i]);
                    _ = AppendSpecialization(g, gens);
                    return new StructSpecialization { Body = x, GenericsMapper = gens };
                }
                else
                {
                    return x;
                }
            }
        }

        if (ns is ILexicalScope lex) return FindStructOrNull(lex.Namespace, name, args);

        if (ns is RootNamespace root)
        {
            foreach (var asm in root.Assemblies)
            {
                foreach (var ti in GetAssemblyType(asm).Select(x => x.GetTypeInfo()).Where(x => TypeNameEquals(x, name)))
                {
                    if (ti.GetGenericArguments().Length != args.Count) continue;

                    //ToDo: List patch
                    var asmx = asm;
                    if (ti == typeof(List<>).GetTypeInfo()) asmx = Assembly.Load("System.Collections");

                    var t = CreateExternStruct(root, ti, asmx);

                    var gens = new GenericsMapper();
                    t.Generics.Each((x, i) => gens[x] = args[i]);
                    return new StructSpecialization { Body = t, GenericsMapper = gens };
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

    public static IStructBody LoadStruct(IManaged ns, string[] name) => FindStructOrNull(ns, name, []) ?? throw new();

    public static IStructBody LoadStruct(IManaged ns, string name) => LoadStruct(ns, [name]);

    public static IEnumerable<LabelCode> AllLabels(List<IOperand> ops) => ops.OfType<LabelCode>().Distinct();

    public static ExternStruct LoadType(RootNamespace root, Type t) => LoadType(root, t.GetTypeInfo());

    public static ExternStruct LoadType(RootNamespace root, TypeInfo ti)
    {
        var st = root.Structs.Where(x => x is ExternStruct sx && sx.Struct == ti).FirstOrDefault();
        if (st is { }) return st.Cast<ExternStruct>();

        Assembly? asmx = null;
        foreach (var asm in root.Assemblies)
        {
            var x = GetAssemblyType(asm).FirstOrDefault(x => x.GetTypeInfo() == ti);
            if (x is { })
            {
                asmx = asm;
                break;
            }
        }
        if (asmx is null) root.Assemblies.Add(asmx = ti.Assembly);

        return CreateExternStruct(root, ti, asmx);
    }

    public static FunctionTypeBody LoadFunctionType(IManaged ns, TypeFunction tf, GenericsMapper gens)
    {
        var func = new FunctionTypeBody();
        tf.Arguments.Each(x => func.Arguments.Add(GetStructType(ns, x, gens)!));
        if (tf.Return is { }) func.Return = GetStructType(ns, tf.Return, gens);
        return func;
    }

    public static IStructBody LoadEnumStruct(IManaged ns, TypeEnum te)
    {
        var e = new EnumStructBody { Namespace = ns };
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

        ti.GenericTypeParameters.Each(x => t.Generics.Add(new TypeGenericsParameter() { Name = x.Name }));
        ti.GetMethods().Each(x => LoadFunction(root, t, x.Name, x));

        return t;
    }

    public static IEnumerable<Type> GetAssemblyType(Assembly asm)
    {
        foreach (var t in asm.GetTypes())
        {
            yield return t;
        }
        foreach (var t in asm.GetReferencedAssemblies().Select(Assembly.Load).Select(GetAssemblyType).Flatten())
        {
            yield return t;
        }
    }

    public static ExternFunction LoadFunction(RootNamespace root, string alias, MethodInfo mi) => LoadFunction(root, root, alias, mi);

    public static ExternFunction LoadFunction(RootNamespace root, INamespace ns, string alias, MethodInfo mi)
    {
        var f = new ExternFunction { Name = alias, Function = mi, Assembly = LoadType(root, mi.DeclaringType!).Assembly };
        ns.Functions.Add(f);
        return f;
    }

    public static RootNamespace GetRootNamespace(IManaged ns) =>
        ns is RootNamespace root ? root
        : ns is IAttachedNamespace lex ? GetRootNamespace(lex.Namespace)
        : ns is IUse use ? use.Uses.OfType<RootNamespace>().First()
        : throw new();

    public static INamespace GetTopLevelNamespace(IManaged ns) =>
        ns is RootNamespace root ? root
        : ns is SourceCodeBody src ? src
        : ns is IAttachedNamespace lex ? GetTopLevelNamespace(lex.Namespace)
        : throw new();

    public static IStructBody? LoadTypeWithoutVoid(RootNamespace root, Type t, GenericsMapper g) =>
        t == typeof(void) ? null
        : t.IsGenericParameter ? g.GetValue(t.Name)
        : LoadType(root, t.GetTypeInfo());

    public static (GenericsMapper GenericsMapper, TypeMapper TypeMapper)? GetGenericsTypeMapperOrNull(Dictionary<GenericsMapper, TypeMapper> sp, GenericsMapper g)
    {
        foreach (var kv in sp)
        {
            if (kv.Key.Keys.All(x => g.ContainsKey(x) && (TypeEquals(kv.Key[x], g[x]) || g[x] is null))) return (kv.Key, kv.Value);
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
