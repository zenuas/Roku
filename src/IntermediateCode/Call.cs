using System.Collections.Generic;

namespace Roku.IntermediateCode
{
    public class Call : Operand
    {
        public ITypedValue Function { get; }
        public ITypedValue? FirstLookup { get; set; }
        public List<ITypedValue> Arguments { get; } = new List<ITypedValue>();

        public Call(ITypedValue f)
        {
            Operator = Operator.Call;
            Function = f;
        }
    }
}
