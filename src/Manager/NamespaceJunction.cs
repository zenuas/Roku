using Extensions;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class NamespaceJunction : INamespace, IUse
    {
        public List<INamespace> Uses { get; }

        public NamespaceJunction(INamespace ns)
        {
            Uses = ns is IUse use ? use.Uses.ToList() : new List<INamespace>();
        }
    }
}
