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
            body.Arguments.Each(x => resolved = ValueInferenceWithEffect(body.Namespace, body.TypeMapper, x.Name, x.Type.Name) || resolved);
            body.Body.Each(x => resolved = OperandTypeInference(body.Namespace, body.TypeMapper, x) || resolved);
            return resolved;
        }

        public static bool OperandTypeInference(INamespace ns, Dictionary<ITypedValue, IStructBody?> m, Operand op)
        {
            switch (op)
            {
                case Call x:
                    return ResolveFunctionWithEffect(ns, m, x);

                default:
                    throw new Exception();
            }
        }

        public static bool ResolveFunctionWithEffect(INamespace ns, Dictionary<ITypedValue, IStructBody?> m, Call call)
        {
            if (m.ContainsKey(call.Function) && m[call.Function] is { }) return false;

            switch (call.Function)
            {
                case VariableValue x:
                    var body = Lookup.FindFunctionOrNull(ns, x.Name, call.Arguments);
                    if (body is { } b)
                    {
                        var fm = new FunctionMapper(b);
                        if (b is FunctionBody fb)
                        {
                            if (fb.Return is { }) fm.TypeMapper[fb.Return] = Lookup.LoadStruct(fb.Namespace, fb.Return.Name);
                            fb.Arguments.Each(x => fm.TypeMapper[x.Name] = Lookup.LoadStruct(fb.Namespace, x.Type.Name));
                        }
                        m[call.Function] = fm;
                    }
                    break;

            }
            return true;
        }

        public static bool ValueInferenceWithEffect(INamespace ns, Dictionary<ITypedValue, IStructBody?> m, ITypedValue v, string type_name)
        {
            if (m.ContainsKey(v) && m[v] is { }) return false;
            m[v] = Lookup.LoadStruct(ns, type_name);
            return true;
        }

        public static bool ValueInferenceWithEffect(INamespace ns, Dictionary<ITypedValue, IStructBody?> m, ITypedValue v)
        {
            if (m.ContainsKey(v) && m[v] is { }) return false;
            m[v] = ToTypedValue(ns, m, v);
            return true;
        }

        public static IStructBody? ToTypedValue(INamespace ns, Dictionary<ITypedValue, IStructBody?> m, ITypedValue v)
        {
            switch (v)
            {
                case StringValue x:
                    m[x] = Lookup.LoadStruct(ns, "String");
                    return m[x];

                case VariableValue x:
                    return m[x];

                default:
                    throw new Exception();
            }
        }
    }
}
