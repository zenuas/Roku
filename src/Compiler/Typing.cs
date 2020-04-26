using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using System;
using System.Collections.Generic;

namespace Roku.Compiler
{
    public static class Typing
    {
        public static void TypeInference(SourceCodeBody src)
        {
            Lookup.AllFunctionBodies(Lookup.AllPrograms(src)).Each(x => TypeInference(x.Body));
        }

        public static void TypeInference(FunctionBody body)
        {
            while (FunctionBodyInference(body)) ;
        }

        public static bool FunctionBodyInference(FunctionBody body)
        {
            var resolved = false;
            body.Arguments.Each((x, i) => resolved = ArgumentInferenceWithEffect(body.Namespace, body.TypeMapper, x.Name, x.Type.Name, i) || resolved);
            body.Body.Each(x => resolved = OperandTypeInference(body.Namespace, body.TypeMapper, x) || resolved);
            return resolved;
        }

        public static bool OperandTypeInference(INamespace ns, Dictionary<ITypedValue, VariableDetail> m, IOperand op)
        {
            switch (op)
            {
                case Code x when x.Operator == Operator.Bind:
                    return LocalValueInferenceWithEffect(ns, m, x.Left!, ToTypedValue(ns, m, x.Right!).Struct);

                case Call x:
                    return ResolveFunctionWithEffect(ns, m, x);

                default:
                    throw new Exception();
            }
        }

        public static bool ResolveFunctionWithEffect(INamespace ns, Dictionary<ITypedValue, VariableDetail> m, Call call)
        {
            if (m.ContainsKey(call.Function.Function) && m[call.Function.Function] is { }) return false;

            switch (call.Function.Function)
            {
                case VariableValue x:
                    var body = Lookup.FindFunctionOrNull(ns, x.Name, call.Function.Arguments.Map(x => ToTypedValue(ns, m, x).Struct).ToList());
                    if (body is { } b)
                    {
                        var fm = new FunctionMapper(b);
                        if (b is FunctionBody fb)
                        {
                            if (fb.Return is { }) fm.TypeMapper[fb.Return] = CreateVariableDetail(Lookup.LoadStruct(fb.Namespace, fb.Return.Name), VariableType.Type);
                            fb.Arguments.Each((x, i) => fm.TypeMapper[x.Name] = CreateVariableDetail(Lookup.LoadStruct(fb.Namespace, x.Type.Name), VariableType.Argument, i));
                        }
                        m[call.Function.Function] = CreateVariableDetail(fm, VariableType.FunctionMapper);
                        //if (call.Return is { }) LocalValueInferenceWithEffect(ns, m, call.Return!);
                    }
                    else
                    {
                        return false;
                    }
                    break;

            }
            if (call.Return is { }) LocalValueInferenceWithEffect(ns, m, call.Return!);
            return true;
        }

        public static bool ArgumentInferenceWithEffect(INamespace ns, Dictionary<ITypedValue, VariableDetail> m, ITypedValue v, string type_name, int index)
        {
            if (m.ContainsKey(v) && m[v].Struct is { }) return false;
            m[v] = CreateVariableDetail(Lookup.LoadStruct(ns, type_name), VariableType.Argument, index);
            return true;
        }

        public static bool LocalValueInferenceWithEffect(INamespace ns, Dictionary<ITypedValue, VariableDetail> m, ITypedValue v, IStructBody? b = null)
        {
            if (m.ContainsKey(v))
            {
                if (m[v].Struct is { } || b is null) return false;
                m[v].Struct = b;
            }
            else
            {
                m[v] = CreateVariableDetail(b, VariableType.LocalVariable, m.Values.Where(x => x.Type == VariableType.LocalVariable).FoldLeft((r, x) => Math.Max(r, x.Index + 1), 0));
            }
            return true;
        }

        public static VariableDetail ToTypedValue(INamespace ns, Dictionary<ITypedValue, VariableDetail> m, ITypedValue v)
        {
            switch (v)
            {
                case NumericValue x:
                    m[x] = CreateVariableDetail(Lookup.LoadStruct(ns, "Int"), VariableType.Type);
                    return m[x];

                case StringValue x:
                    m[x] = CreateVariableDetail(Lookup.LoadStruct(ns, "String"), VariableType.Type);
                    return m[x];

                case VariableValue x:
                    return m[x];

                case TemporaryValue x:
                    return m[x];

                default:
                    throw new Exception();
            }
        }

        public static VariableDetail CreateVariableDetail(IStructBody? b, VariableType type, int index = 0) => new VariableDetail { Struct = b, Type = type, Index = index };
    }
}
