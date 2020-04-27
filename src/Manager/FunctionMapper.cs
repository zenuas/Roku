using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class FunctionMapper : IStructBody
    {
        public IFunctionBody Function { get; }
        public string Name { get => Function.Name; }
        public Dictionary<ITypedValue, VariableDetail> TypeMapper { get; } = new Dictionary<ITypedValue, VariableDetail>();

        public FunctionMapper(IFunctionBody f)
        {
            Function = f;
        }

        public override string ToString() => Function.ToString()!;
    }
}
