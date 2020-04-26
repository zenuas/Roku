using System.Collections.Generic;

namespace Roku.IntermediateCode
{
    public class Call : IOperand
    {
        public Operator Operator { get; set; } = Operator.Call;
        public ITypedValue Function { get; }
        public ITypedValue? FirstLookup { get; set; }
        public List<ITypedValue> Arguments { get; } = new List<ITypedValue>();

        public Call(ITypedValue f)
        {
            Function = f;
        }
    }
}
