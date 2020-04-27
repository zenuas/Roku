using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class StructBody : IStructBody
    {
        public string Name { get; }
        public List<IOperand> Body { get; } = new List<IOperand>();

        public StructBody(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}
