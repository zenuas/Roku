using Roku.TypeSystem;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class FunctionMapper : Dictionary<string, IType?>, IType
    {
        public IFunctionBody Function { get; }
        public string Name { get => Function.Name; }

        public FunctionMapper(IFunctionBody f)
        {
            Function = f;
        }
    }
}
