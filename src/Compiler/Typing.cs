using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;

namespace Roku.Compiler
{
    public static class Typing
    {
        public static void TypeInference(SourceCodeBody src)
        {
            while (Lookup.AllFunctionBodies(Lookup.AllPrograms(src)).FoldLeft((r, x) => FunctionBodyInference(x.Body, x.Source) || r, false)) ;
        }

        public static bool FunctionBodyInference(FunctionBody body, SourceCodeBody src)
        {
            var resolved = false;
            body.Arguments.Each(x => resolved = ValueInferenceWithEffect(x.Type, x.Type.Name, src) || resolved);
            body.Arguments.Each(x => resolved = ValueInferenceWithEffect(x.Name, x.Type.Name, src) || resolved);
            body.Body.Each(x => resolved = OperandTypeInference(body, x, src) || resolved);
            return resolved;
        }

        public static bool OperandTypeInference(FunctionBody body, Operand op, SourceCodeBody src)
        {
            var resolved = false;
            Lookup.AllValues(op).Each(x => resolved = ValueInferenceWithEffect(body, x, src) || resolved);

            switch (op)
            {
                case Call x:
                    resolved = ResolveFunctionWithEffect(x, src) || resolved;
                    break;
            }
            return false;
        }

        public static bool ResolveFunctionWithEffect(Call call, SourceCodeBody src)
        {
            if (call.Function is null)
            {
                call.Function = Lookup.FindFunction(src, call.Name, call.Arguments);
            }
            return false;
        }

        public static bool ValueInferenceWithEffect(ITypedValue v, string type_name, SourceCodeBody src)
        {
            if (v.Type is { }) return false;
            v.Type = Lookup.LoadStruct(src, type_name);
            return true;
        }

        public static bool ValueInferenceWithEffect(FunctionBody body, ITypedValue v, SourceCodeBody src)
        {
            if (v.Type is { }) return false;
            return false;
        }
    }
}
