using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class StructBody : IStructBody
    {
        public string Name { get; }
        public Dictionary<string, ITypedValue> Members { get; } = new Dictionary<string, ITypedValue>();
        public List<IOperand> Constructor { get; } = new List<IOperand>();

        public StructBody(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}
