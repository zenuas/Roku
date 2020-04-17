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
            return body.Body.FoldLeft((r, x) => OperandTypeInference(body, x, src) || r, false);
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

        public static bool ValueInferenceWithEffect(FunctionBody body, ITypedValue v, SourceCodeBody src)
        {
            if (v.Type is { }) return false;
            return false;
        }
    }
}
