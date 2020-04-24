using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class FunctionMapper : IStructBody
    {
        public IFunctionBody Function { get; }
        public string Name { get => Function.Name; }
        public Dictionary<ITypedValue, IStructBody?> TypeMapper { get; } = new Dictionary<ITypedValue, IStructBody?>();

        public FunctionMapper(IFunctionBody f)
        {
            Function = f;
        }
    }
}
