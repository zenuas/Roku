using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public interface ILexicalScope
    {
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; }
        public Dictionary<string, ITypedValue> LexicalScope { get; }
        public List<Operand> Body { get; }
    }
}
