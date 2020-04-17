using Roku.IntermediateCode;
using Roku.TypeSystem;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class FunctionBody : IFunctionBody
    {
        public IFunction Function { get; set; }
        public List<Operand> Body { get; } = new List<Operand>();

        public FunctionBody(IFunction f)
        {
            Function = f;
        }
    }
}
