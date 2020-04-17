using Roku.TypeSystem;
using System.Collections.Generic;

namespace Roku.IntermediateCode
{
    public class Call : Operand
    {
        public string Name { get; }
        public ITypedValue? FirstLookup { get; set; }
        public IFunction? Function { get; set; }
        public List<ITypedValue> Arguments { get; } = new List<ITypedValue>();

        public Call(string name)
        {
            Operator = Operator.Call;
            Name = name;
        }
    }
}
