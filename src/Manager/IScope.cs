using Roku.IntermediateCode;
using Roku.TypeSystem;
using System.Collections.Generic;

namespace Roku.Manager
{
    public interface IScope
    {
        public INamespace Namespace { get; }
        public IScope? Parent { get; }
        public Dictionary<string, IType?> Scope { get; }
        public List<Operand> Body { get; }
    }
}
