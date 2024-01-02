using Mina.Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Compiler;

public static partial class Typing
{
    public static bool OperandTypeInference(IManaged ns, TypeMapper m, IOperand op)
    {
        switch (op)
        {
            case BindCode x:
                var resolve = false;
                if (x.Return is PropertyValue prop)
                {
                    resolve = LocalValueInferenceWithEffect(m, prop, ToTypedValue(ns, m, prop).Struct);
                }
                return LocalValueInferenceWithEffect(m, x.Return!, ToTypedValue(ns, m, x.Value!).Struct) || resolve;

            case Call x:
                return ResolveFunctionWithEffect(ns, m, x);

            case TypeBind x:
                return LocalValueInferenceWithEffect(m, x.Name, TypeDefinitionToStructBody(ns, m, x.Type));

            case TypeReferenceBind x:
                return LocalValueInferenceWithEffect(m, x.Name, x.Struct);

            case IfCastCode x:
                return LocalValueInferenceWithEffect(m, x.Name, Lookup.LoadStruct(ns, x.Type.Name));

            case IfCode _:
            case GotoCode _:
            case LabelCode _:
                return false;
        }
        throw new();
    }

    public static IStructBody TypeDefinitionToStructBody(IManaged ns, TypeMapper m, ITypeDefinition t)
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
                    return Lookup.FindStructOrNull(ns, name, args.ToList()) ?? throw new();
                }

            default:
                return Lookup.LoadStruct(ns, t.Name);
        }
    }

    public static IStructBody FindTypeMapperToGenerics(TypeMapper m, string name) => m.Where(x => x.Key is ITypeDefinition t && t.Name == name).First().Value.Struct!;

    public static bool LocalValueInferenceWithEffect(TypeMapper m, IEvaluable v, IStructBody? b = null)
    {
        if (m.TryGetValue(v, out var value))
        {
            if ((value.Struct is { } p && (IsDecideType(p) || p is FunctionMapper)) || b is null) return false;
            value.Struct = b;

            if (v is PropertyValue prop)
            {
                if (m[prop.Left].Struct is StructBody sb)
                {
                    switch (sb.Type)
                    {
                        case StructBodyTypes.CoroutineLocal:
                            sb.Body.Add(new TypeBind { Name = sb.LexicalScope[prop.Right], Type = new TypeValue { Name = b.Name } });
                            break;

                        case StructBodyTypes.Capture:
                            var member = sb.Members[prop.Right];
                            sb.Body.Add(new TypeReferenceBind { Name = member, Struct = b });
                            break;
                    }
                }
            }
        }
        else if (b is AnonymousFunctionBody anon)
        {
            var fm = new FunctionMapper { Function = anon };
            m[v] = CreateVariableDetail(v.ToString()!, fm, VariableType.FunctionMapper);
        }
        else
        {
            m[v] = CreateVariableDetail(v.ToString()!, b, VariableType.LocalVariable, m.Values.Where(x => x.Type == VariableType.LocalVariable).Aggregate(0, (r, x) => Math.Max(r, x.Index + 1)));
        }
        return true;
    }

    public static ILexicalScope? GetCaptured(IManaged ns, VariableValue v)
    {
        if (ns is FunctionBody fb)
        {
            if (fb.Capture.TryGetValue(v, out var value)) return value;
        }
        return null;
    }

    public static VariableDetail ToTypedValue(IManaged ns, TypeMapper m, IEvaluable v, bool nonamespace = false)
    {
        if (m.TryGetValue(v, out var value) && value.Struct is { } p && IsDecideType(p)) return value;

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
                {
                    var scope = GetCaptured(ns, x);
                    if (scope is null) return m[x];
                    var reciever = m.First(kv => kv.Value.Struct?.Name == Definition.ScopeToUniqueName(scope)).Key;
                    var prop = m[x] = CreateVariableDetail(x.Name, GetPropertyType(m, reciever, x.Name), VariableType.Property);
                    return m[x];
                }

            case TemporaryValue x:
                return m[x];

            case PropertyValue x:
                {
                    _ = ToTypedValue(ns, m, x.Left);
                    var prop = m[x] = CreateVariableDetail(x.Right, GetPropertyType(m, x.Left, x.Right), VariableType.Property);
                    prop.Reciever = x.Left;
                    return m[x];
                }

            case ArrayContainer x:
                x.Values.Each(value => ToTypedValue(ns, m, value));
                var gens = new List<IStructBody>() { x.Values.Count > 0 ? m[x.Values[0]].Struct! : new IndefiniteBody() };
                m[x] = CreateVariableDetail("", Lookup.FindStructOrNull(ns, ["System", "Collections", "Generic", "List"], gens), VariableType.PrimitiveValue);
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
                m[x] = CreateVariableDetail("", Lookup.GetRootNamespace(ns).Structs.OfType<AnonymousFunctionBody>().First(f => f.Name == x.Name), VariableType.PrimitiveValue);
                return m[x];
        }
        throw new();
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
                throw new();
        }
    }

    public static IStructBody? GetPropertyType(IStructBody left, string property, GenericsMapper? g)
    {
        switch (left)
        {
            case StructBody x:
                if (x.Members.TryGetValue(property, out var value))
                {
                    var m = g is null ? x.SpecializationMapper.First().Value : Lookup.GetGenericsTypeMapperOrNull(x.SpecializationMapper, g)!.Value.TypeMapper;
                    if (m.ContainsKey(value)) return m[value].Struct;
                }
                break;

            default:
                throw new();
        }
        return null;
    }
}
