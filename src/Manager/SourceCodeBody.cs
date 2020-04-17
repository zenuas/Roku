using Extensions;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class SourceCodeBody : INamespace
    {
        public INamespace Current { get; }
        public List<IFunctionBody> Functions => Current.Cast<NamespaceManager>().Functions;
        public List<INamespace> Uses { get; } = new List<INamespace>();

        public SourceCodeBody(NamespaceManager ns)
        {
            Current = ns;
        }
    }
}
