using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class StructBody : IStructBody
    {
        public string Name { get; }
        public List<Operand> Body { get; } = new List<Operand>();

        public StructBody(string name)
        {
            Name = name;
        }
    }
}
