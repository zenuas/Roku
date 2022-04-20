﻿using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Compiler;

public static partial class Typing
{
    public static FunctionSpecialization? FindCurrentFunction(INamespace ns, TypeMapper m, VariableValue x, List<IStructBody?> args)
    {
        if (m.ContainsKey(x) && m[x].Struct is { } p)
        {
            switch (p)
            {
                case FunctionTypeBody ftb: return new FunctionSpecialization(ftb, Lookup.FunctionArgumentsEquals(ns, ftb, args).GenericsMapper);
                case AnonymousFunctionBody afb: return new FunctionSpecialization(afb, Lookup.IfFunctionArgumentsEquals_ThenAppendSpecialization(ns, afb, args)!.GenericsMapper);
                case FunctionMapper fm:
                    if (fm.Function is AnonymousFunctionBody)
                    {
                        var sp = Lookup.IfFunctionArgumentsEquals_ThenAppendSpecialization(ns, fm.Function, args)!;
                        return new FunctionSpecialization(fm.Function, sp.GenericsMapper);
                    }
                    break;
            }
        }
        return Lookup.FindFunctionOrNull(ns, x.Name, args);
    }

    public static bool IsDecideFunction(TypeMapper m, Call call)
    {
        if (m.ContainsKey(call.Function.Function) && m[call.Function.Function].Struct is { } p)
        {
            if (call.Return is { } && p is FunctionMapper fm && fm.Function is AnonymousFunctionBody afb && afb.Return is TypeImplicit)
            {
                return false;
            }
            if (IsDecideFunction(p) && (call.Return is null || (call.Return is { } rx && m.ContainsKey(rx) && m[rx].Struct is { } rs && IsDecideType(rs))))
            {
                return true;
            }
        }
        return false;
    }

    public static bool ResolveFunctionWithEffect(INamespace ns, TypeMapper m, Call call)
    {
        if (IsDecideFunction(m, call)) return false;

        var resolve = false;
        var lookupns = ns;
        if (call.Function.FirstLookup is { } receiver)
        {
            if (m.ContainsKey(receiver) && m[receiver] is { } r)
            {
                if (r.Struct is { }) lookupns = GetStructNamespace(ns, r.Struct);
                if ((r.Type == VariableType.LocalVariable || r.Type == VariableType.Argument) && !call.Function.ReceiverToArgumentsInserted)
                {
                    call.Function.Arguments.Insert(0, receiver);
                    call.Function.ReceiverToArgumentsInserted = true;
                }
            }
        }
        var args = call.Function.Arguments.Select(x => ToTypedValue(ns, m, x, true).Struct).ToList();

        switch (call.Function.Function)
        {
            case VariableValue x when call.Return is null && call.Function.FirstLookup is null && x.Name == "return":
                {
                    //var ret = new EmbeddedFunction("return", null, args.Map(x => x?.Name!).ToArray()) { OpCode = (args) => $"{(args.Length == 0 ? "" : args[0] + "\n")}ret" };
                    var r = ns.Cast<FunctionBody>().Return;
                    var ret = new EmbeddedFunction("return", null, r is { } ? new ITypeDefinition[] { r } : new ITypeDefinition[] { }) { OpCode = (_, args) => $"{(args.Length == 0 ? "" : args[0] + "\n")}ret" };
                    Lookup.AppendSpecialization(ret, new GenericsMapper());
                    var fm = new FunctionMapper(ret);
                    if (r is TypeGenericsParameter gen) fm.TypeMapper[gen] = CreateVariableDetail("", m[gen].Struct ?? (args.Count > 0 ? args[0] : null), VariableType.TypeParameter);
                    else if (r is TypeSpecialization gv && m.ContainsKey(r) && m[r].Struct is StructSpecialization sp && sp.Body is ISpecialization sp2) gv.Generics.Each((x, i) => fm.TypeMapper[x] = CreateVariableDetail("", sp.GenericsMapper[sp2.Generics[i]], VariableType.TypeParameter));

                    m[x] = CreateVariableDetail("", fm, VariableType.FunctionMapper);
                    return true;
                }

            case VariableValue x:
                {
                    var caller = FindCurrentFunction(lookupns, m, x, args);
                    if (caller is null) break;

                    var fm = CreateFunctionMapper(ns, caller);
                    IStructBody? ret = null;
                    if (caller.Body is FunctionBody fb)
                    {
                        if (fb.Return is { }) ret = fm.TypeMapper[fb.Return].Struct;
                        fb.Arguments.Each((x, i) => Feedback(args[i], fm.TypeMapper[x.Name].Struct));
                    }
                    else if (caller.Body is FunctionTypeBody ftb)
                    {
                        ret = ftb.Return;
                    }
                    else if (caller.Body is ExternFunction fx)
                    {
                        ret = Lookup.LoadTypeWithoutVoid(Lookup.GetRootNamespace(ns), fx.Function.ReturnType, caller.GenericsMapper);
                    }
                    else if (caller.Body is EmbeddedFunction ef)
                    {
                        if (ef.Return is { }) ret = fm.TypeMapper[ef.Return].Struct;
                    }
                    else if (caller.Body is AnonymousFunctionBody afb)
                    {
                        if (caller.GenericsMapper.Count > 0 && m.ContainsKey(x))
                        {
                            call.Function.Function = x = new VariableValue() { Name = x.Name };
                        }
                    }
                    if (m.ContainsKey(x))
                    {
                        m[x].Struct = fm;
                    }
                    else
                    {
                        m[x] = CreateVariableDetail("", fm, VariableType.FunctionMapper);
                    }
                    if (call.Return is { }) LocalValueInferenceWithEffect(ns, m, call.Return!, ret);
                    resolve = true;
                }
                break;

            case TypeSpecialization x:
                {
                    var gens = x.Generics.Select(x => Lookup.GetStructType(ns, x, m)!).ToList();
                    var body = Lookup.FindStructOrNull(ns, GetStructNames(m, x).ToArray(), gens);
                    if (body is null) break;

                    var em = new EmbeddedFunction(x.ToString(), x.ToString()) { OpCode = (_, args) => $"newobj instance void {CodeGenerator.GetStructName(body)}::.ctor()" };
                    Lookup.AppendSpecialization(em, new GenericsMapper());
                    var fm = new FunctionMapper(em);
                    m[x] = CreateVariableDetail("", fm, VariableType.FunctionMapper);
                    if (call.Return is { }) LocalValueInferenceWithEffect(ns, m, call.Return!, body);
                    resolve = true;
                }
                break;

            default:
                throw new Exception();

        }
        return call.Return is { } ? LocalValueInferenceWithEffect(ns, m, call.Return!) || resolve : resolve;
    }

    public static FunctionMapper CreateFunctionMapper(INamespace ns, FunctionSpecialization caller)
    {
        var fm = new FunctionMapper(caller.Body);
        caller.GenericsMapper.Each(p => fm.TypeMapper[p.Key] = Typing.CreateVariableDetail(p.Key.Name, p.Value, VariableType.TypeParameter));

        if (caller.Body is FunctionBody fb)
        {
            if (fb.Return is { } && !fm.TypeMapper.ContainsKey(fb.Return)) fm.TypeMapper[fb.Return] = Typing.CreateVariableDetail("", Lookup.GetStructType(fb.Namespace, fb.Return, caller.GenericsMapper), VariableType.Type);
            fb.Arguments.Each((x, i) => fm.TypeMapper[x.Name] = Typing.CreateVariableDetail(x.Name.Name, Lookup.GetStructType(fb.Namespace, x.Type, caller.GenericsMapper), VariableType.Argument, i));
        }
        else if (caller.Body is EmbeddedFunction ef)
        {
            if (ef.Return is { } && !fm.TypeMapper.ContainsKey(ef.Return)) fm.TypeMapper[ef.Return] = Typing.CreateVariableDetail("", Lookup.LoadStruct(ns, ef.Return.Name), VariableType.Type);
            ef.Arguments.Each((x, i) => fm.TypeMapper[new VariableValue() { Name = $"${i}" }] = Typing.CreateVariableDetail($"${i}", Lookup.GetStructType(ns, x, caller.GenericsMapper), VariableType.Argument, i));
        }
        return fm;
    }

    public static INamespace GetStructNamespace(INamespace ns, IStructBody body)
    {
        switch (body)
        {
            case ExternStruct x:
                return new NamespaceJunction(ns).Return(j => j.Uses.Add(x));

            case StructBody x:
                return x.Namespace;

            case StructSpecialization x:
                return new NamespaceJunction(ns).Return(j => j.Uses.Add(x));
        }
        throw new Exception();
    }

    public static IEnumerable<string> GetStructNames(TypeMapper m, IEvaluable e)
    {
        if (e is TypeSpecialization g)
        {
            if (g.Type is PropertyValue prop) return GetStructNames(m, prop.Left).Concat(prop.Right);
            return new string[] { g.Type.ToString()! };
        }
        if (m.ContainsKey(e) && m[e].Struct is NamespaceBody ns) return GetNamespaceNames(ns);
        throw new Exception();
    }

    public static IEnumerable<string> GetNamespaceNames(NamespaceBody ns) => ns.Parent is { } p ? GetNamespaceNames(p).Concat(ns.Name) : new string[] { ns.Name };

    public static void Feedback(IStructBody? left, IStructBody? right)
    {
        if (right is null) return;

        if (left is NumericStruct num)
        {
            if (num.Types.Contains(right)) num.Types.RemoveAll(x => x != right);
        }
        else if (left is IGenericsMapper lgm &&
            !Lookup.IsFixedStruct(left) &&
            right is IGenericsMapper rgm)
        {
            foreach (var kvv in lgm.GenericsMapper.Keys.Select(x => (Key: x, Left: lgm.GenericsMapper[x], Right: rgm.GenericsMapper[x])).ToArray())
            {
                if (kvv.Left is IndefiniteBody && !(kvv.Right is IndefiniteBody))
                {
                    lgm.GenericsMapper[kvv.Key] = kvv.Right;
                }
            }
        }
        else if (left is FunctionMapper fm)
        {
            if (fm.Function is AnonymousFunctionBody anon)
            {
                var g = new GenericsMapper();
                Lookup.AppendSpecialization(anon, g);
                if (anon.IsImplicit && anon.Return is TypeImplicit imp)
                {
                    if (right is FunctionTypeBody ftb)
                    {
                        if (ftb.Return is null)
                        {
                            anon.Return = null;
                            anon.IsImplicit = false;
                        }
                        else
                        {
                            fm.TypeMapper[imp] = Typing.CreateVariableDetail(imp.Name, ftb.Return, VariableType.TypeParameter);
                        }
                    }
                }
                else if (anon.Return is { } ret)
                {
                    fm.TypeMapper[ret] = Typing.CreateVariableDetail(ret.Name, Lookup.GetStructType(anon.Namespace, ret, g), VariableType.TypeParameter);
                }
            }
        }
    }
}