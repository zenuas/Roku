using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Roku.IntermediateCode
{
    public class FunctionCallValue : ITypedValue
    {
        public ITypedValue Function { get; }
        public ITypedValue? FirstLookup { get; set; }
        public List<ITypedValue> Arguments { get; } = new List<ITypedValue>();

        public FunctionCallValue(ITypedValue f)
        {
            Function = f;
        }

        public override string ToString() => $"{(FirstLookup is null ? "" : FirstLookup.ToString() + ".")}{Function}({Arguments.Map(x => x.ToString()!).Join(", ")})";
    }
}
