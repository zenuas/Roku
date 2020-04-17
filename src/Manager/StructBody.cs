using Roku.IntermediateCode;
using Roku.TypeSystem;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class StructBody
    {
        public IType Struct { get; set; }
        public List<Operand> Body { get; } = new List<Operand>();

        public StructBody(IType t)
        {
            Struct = t;
        }
    }
}
