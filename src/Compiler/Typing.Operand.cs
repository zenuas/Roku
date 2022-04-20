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
    public static bool OperandTypeInference(INamespace ns, TypeMapper m, IOperand op)
    {
        switch (op)
        {
            case Code x when x.Operator == Operator.Bind:
                var resolve = false;
                if (x.Return is PropertyValue prop)
                {
                    resolve = LocalValueInferenceWithEffect(ns, m, prop, ToTypedValue(ns, m, prop).Struct);
                }
                return LocalValueInferenceWithEffect(ns, m, x.Return!, ToTypedValue(ns, m, x.Left!).Struct) || resolve;

            case Call x:
                return ResolveFunctionWithEffect(ns, m, x);

            case TypeBind x:
                return LocalValueInferenceWithEffect(ns, m, x.Name, TypeDefinitionToStructBody(ns, m, x.Type));

            case IfCastCode x:
                return LocalValueInferenceWithEffect(ns, m, x.Name, Lookup.LoadStruct(ns, x.Type.Name));

            case IfCode _:
            case GotoCode _:
            case LabelCode _:
                return false;
        }
        throw new Exception();
    }

    public static IStructBody TypeDefinitionToStructBody(INamespace ns, TypeMapper m, ITypeDefinition t)
    {
        switch (t)
        {
            case TypeGenericsParameter:
                return FindTypeMapperToGenerics(m, t.Name);

            case TypeEnum te:
                return Lookup.LoadEnumStruct(ns, te);

            case TypeValue tv:
                return Lookup.LoadStruct(ns, Lookup.GetTypeNames(tv));

            case TypeSpecialization ts:
                {
                    var args = ts.Generics.Select(g => TypeDefinitionToStructBody(ns, m, g));
                    var name = Lookup.GetTypeNames(ts.Type);
                    return Lookup.FindStructOrNull(ns, name, args.ToList()) ?? throw new Exception();
                }

            default:
                return Lookup.LoadStruct(ns, t.Name);
        }
    }

    public static IStructBody FindTypeMapperToGenerics(TypeMapper m, string name) => m.Where(x => x.Key is ITypeDefinition t && t.Name == name).First().Value.Struct!;

    public static bool LocalValueInferenceWithEffect(INamespace ns, TypeMapper m, IEvaluable v, IStructBody? b = null)
    {
        if (m.ContainsKey(v))
        {
            if ((m[v].Struct is { } p && (IsDecideType(p) || p is FunctionMapper)) || b is null) return false;
            m[v].Struct = b;

            if (v is PropertyValue prop)
            {
                if (m[prop.Left].Struct is StructBody sb && sb.IsCoroutineLocal)
                {
                    sb.Body.Add(new TypeBind(sb.LexicalScope[prop.Right], new TypeValue() { Name = b.Name }));
                }
            }
        }
        else if (b is AnonymousFunctionBody anon)
        {
            var fm = new FunctionMapper(anon);
            m[v] = CreateVariableDetail(v.ToString()!, fm, VariableType.FunctionMapper);
        }
        else
        {
            m[v] = CreateVariableDetail(v.ToString()!, b, VariableType.LocalVariable, m.Values.Where(x => x.Type == VariableType.LocalVariable).FoldLeft((r, x) => Math.Max(r, x.Index + 1), 0));
        }
        return true;
    }

    public static VariableDetail ToTypedValue(INamespace ns, TypeMapper m, IEvaluable v, bool nonamespace = false)
    {
        if (m.ContainsKey(v) && m[v].Struct is { } p && IsDecideType(p)) return m[v];

        switch (v)
        {
            case NumericValue x:
                if (m.ContainsKey(v) && m[x].Struct is NumericStruct) return m[x];
                m[x] = CreateNumericType(ns);
                return m[x];

            case FloatingNumericValue x:
                if (m.ContainsKey(v) && m[x].Struct is NumericStruct) return m[x];
                m[x] = CreateFloatingNumericType(ns);
                return m[x];

            case BooleanValue x:
                m[x] = CreateVariableDetail("", Lookup.LoadStruct(ns, "Bool"), VariableType.PrimitiveValue);
                return m[x];

            case NullValue x:
                m[x] = CreateVariableDetail("", Lookup.LoadStruct(ns, "Null"), VariableType.PrimitiveValue);
                return m[x];

            case StringValue x:
                m[x] = CreateVariableDetail("", Lookup.LoadStruct(ns, "String"), VariableType.PrimitiveValue);
                return m[x];

            case VariableValue x:
                return m[x];

            case TemporaryValue x:
                return m[x];

            case PropertyValue x:
                _ = ToTypedValue(ns, m, x.Left);
                var prop = m[x] = CreateVariableDetail(x.Right, GetPropertyType(m, x.Left, x.Right), VariableType.Property);
                prop.Reciever = x.Left;
                return m[x];

            case ArrayContainer x:
                x.Values.Each(value => ToTypedValue(ns, m, value));
                var gens = new List<IStructBody>() { x.Values.Count > 0 ? m[x.Values[0]].Struct! : new IndefiniteBody() };
                m[x] = CreateVariableDetail("", Lookup.FindStructOrNull(ns, new string[] { "System", "Collections", "Generic", "List" }, gens), VariableType.PrimitiveValue);
                return m[x];

            case TypeValue x:
                if (nonamespace)
                {
                    m[x] = CreateVariableDetail(x.Name, Lookup.LoadStruct(ns, x.Name), VariableType.PrimitiveValue);
                }
                else
                {
                    m[x] = CreateVariableDetail(x.Name, new NamespaceBody() { Name = x.Name }, VariableType.Namespace);
                }
                return m[x];

            case FunctionReferenceValue x:
                m[x] = CreateVariableDetail("", Lookup.GetRootNamespace(ns).Structs.OfType<AnonymousFunctionBody>().FindFirst(f => f.Name == x.Name), VariableType.PrimitiveValue);
                return m[x];
        }
        throw new Exception();
    }

    public static IStructBody? GetPropertyType(TypeMapper m, IEvaluable receiver, string property)
    {
        var left = m[receiver].Struct;
        if (left is null) return null;

        switch (left)
        {
            case StructBody x:
                return GetPropertyType(x, property, null);

            case StructSpecialization x:
                return GetPropertyType(x.Body, property, x.GenericsMapper);

            case NamespaceBody x:
                return new NamespaceBody() { Name = property, Parent = x };

            default:
                throw new Exception();
        }
    }

    public static IStructBody? GetPropertyType(IStructBody left, string property, GenericsMapper? g)
    {
        switch (left)
        {
            case StructBody x:
                if (x.Members.ContainsKey(property))
                {
                    var m = g is null ? x.SpecializationMapper.First().Value : Lookup.GetGenericsTypeMapperOrNull(x.SpecializationMapper, g)!.Value.TypeMapper;
                    if (m.ContainsKey(x.Members[property])) return m[x.Members[property]].Struct;
                }
                break;

            default:
                throw new Exception();
        }
        return null;
    }
}