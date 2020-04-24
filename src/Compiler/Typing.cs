using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using Roku.TypeSystem;
using System;

namespace Roku.Compiler
{
    public static class Typing
    {
        public static TypeMapper TypeInference(FunctionBody body)
        {
            var m = new TypeMapper();
            while (FunctionBodyInference(body, m)) ;
            return m;
        }

        public static bool FunctionBodyInference(FunctionBody body, TypeMapper m)
        {
            var resolved = false;
            body.Arguments.Each(x => resolved = ValueInferenceWithEffect(body.Namespace, m, x.Name, x.Type.Name) || resolved);
            body.Body.Each(x => resolved = OperandTypeInference(body.Namespace, m, x) || resolved);
            return resolved;
        }

        public static bool OperandTypeInference(INamespace ns, TypeMapper m, Operand op)
        {
            switch (op)
            {
                case Call x:
                    return ResolveFunctionWithEffect(ns, m, x);

                default:
                    throw new Exception();
            }
        }

        public static bool ResolveFunctionWithEffect(INamespace ns, TypeMapper m, Call call)
        {
            if (m.ContainsKey(call.Function) && m[call.Function] is { }) return false;

            switch (call.Function)
            {
                case VariableValue x:
                    var body = Lookup.FindFunction(ns, x.Name, call.Arguments);
                    if (body is { } fb) m[call.Function] = new FunctionMapper(fb);
                    break;

            }
            return false;
        }

        public static bool ValueInferenceWithEffect(INamespace ns, TypeMapper m, ITypedValue v, string type_name)
        {
            if (m.ContainsKey(v) && m[v] is { }) return false;
            m[v] = Lookup.LoadStruct(ns, type_name);
            return true;
        }

        public static bool ValueInferenceWithEffect(INamespace ns, TypeMapper m, ITypedValue v)
        {
            if (m.ContainsKey(v) && m[v] is { }) return false;
            m[v] = ToTypedValue(ns, m, v);
            return true;
        }

        public static IType? ToTypedValue(INamespace ns, TypeMapper m, ITypedValue v)
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
