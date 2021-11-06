using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;
using System;
using System.Linq;

namespace Roku.Compiler
{
    public static partial class Typing
    {
        public static bool TypeInference(IFunctionBody body)
        {
            var resolved = FunctionBodyInference(body);
            SpecializationNumericDecide(body);
            _ = FunctionBodyInference(body);

            foreach (var mapper in body.SpecializationMapper.Values)
            {
                if (body.Body.OfType<IfCastCode>().FindFirstOrNull(x => Lookup.IsValueType(mapper[x.Condition].Struct)) is { })
                {
                    LocalValueInferenceWithEffect(body.Namespace, mapper, mapper.CastBoxCondition, Lookup.LoadType(Lookup.GetRootNamespace(body.Namespace), typeof(object)));
                }
            }
            return resolved;
        }

        public static bool FunctionBodyInference(IFunctionBody body)
        {
            var resolved = false;
            var keys = body.SpecializationMapper.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++)
            {
                var g = keys[i];
                var m = body.SpecializationMapper[g];
                body.Arguments.Each((x, i) => resolved = ArgumentInferenceWithEffect(body.Namespace, m, x.Name, x.Type, i) || resolved);
                if (body.Return is { } x && !(x is TypeImplicit)) resolved = TypeInferenceWithEffect(body.Namespace, m, x, x) || resolved;
                body.Body.Each(x => resolved = OperandTypeInference(body, m, x) || resolved);

                if (body is AnonymousFunctionBody afb && afb.IsImplicit && body.Return is TypeImplicit v)
                {
                    if (!(m.ContainsKey(v) && m[v].Struct is { } p && IsDecideType(p)) &&
                        m.Keys.FindFirstOrNull(x => x is ImplicitReturnValue) is { } imp)
                    {
                        if (m[imp].Struct is { } impx)
                        {
                            m[v] = CreateVariableDetail("", m[imp].Struct, VariableType.Type);
                        }
                        else
                        {
                            afb.Return = null;
                            afb.Body
                                .OfType<IReturnBind>()
                                .Where(x => x.Return is ImplicitReturnValue)
                                .Each(x => x.Return = null);
                        }
                        resolved = true;
                    }
                }

                if (body.Return is TypeGenericsParameter r)
                {
                    if (!(m.ContainsKey(r) && m[r].Struct is { } p && IsDecideType(p)))
                    {
                        var rets = m.Values
                            .Where(x => x.Struct is FunctionMapper fm && fm.Function is EmbeddedFunction && fm.Name == "return")
                            .Select(x => x.Struct!.Cast<FunctionMapper>().TypeMapper)
                            .Where(x => x.ContainsKey(r))
                            .Select(x => x[r].Struct)
                            .Distinct()
                            .ToList();

                        if (rets.Count == 1)
                        {
                            g[r] = rets[0];
                            m[r] = CreateVariableDetail("", rets[0], VariableType.TypeParameter);
                            resolved = true;
                        }
                        else if (rets.Count > 1)
                        {

                        }
                    }
                }
            }
            return resolved;
        }

        public static bool ArgumentInferenceWithEffect(INamespace ns, TypeMapper m, IEvaluable v, ITypeDefinition type, int index)
        {
            if (m.ContainsKey(v) && m[v].Struct is { } p && IsDecideType(p)) return false;
            switch (type)
            {
                case TypeGenericsParameter g:
                    if (!m.ContainsKey(type)) m[type] = CreateVariableDetail(g.Name, new GenericsParameter(g.Name), VariableType.TypeParameter, index);
                    m[v] = CreateVariableDetail($"${index}", m[type].Struct, VariableType.Argument, index);
                    break;

                case TypeValue t:
                    m[v] = CreateVariableDetail($"${index}", Lookup.LoadStruct(ns, t.Name), VariableType.Argument, index);
                    break;

                case TypeSpecialization sp:
                    m[v] = CreateVariableDetail($"${index}", Lookup.FindStructOrNull(ns, Lookup.GetTypeNames(sp.Type), sp.Generics.Select(x => Lookup.GetStructType(ns, x, m)!).ToList()), VariableType.Argument, index);
                    break;

                case TypeFunction tf:
                    m[v] = CreateVariableDetail($"${index}", Lookup.LoadFunctionType(ns, tf, Lookup.TypeMapperToGenericsMapper(m)), VariableType.Argument, index);
                    break;

                case TypeEnum te:
                    m[v] = CreateVariableDetail($"${index}", Lookup.LoadEnumStruct(ns, te), VariableType.Argument, index);
                    break;

                default:
                    throw new Exception();
            }
            return true;
        }

        public static bool TypeInferenceWithEffect(INamespace ns, TypeMapper m, IEvaluable v, ITypeDefinition type)
        {
            if (m.ContainsKey(v) && m[v].Struct is { } p && IsDecideType(p)) return false;
            var s = Lookup.GetStructType(ns, type, m);
            m[v] = CreateVariableDetail("", s, type is TypeGenericsParameter ? VariableType.TypeParameter : VariableType.Type);
            return true;
        }
    }
}
