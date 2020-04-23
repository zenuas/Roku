using Roku.TypeSystem;
using System.Collections.Generic;

namespace Roku.Manager
{
    public interface ILexicalScope
    {
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; }
        public Dictionary<string, IType?> LexicalScope { get; }
    }
}
