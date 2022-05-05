using System.Collections.Generic;
using System.Linq;

namespace Roku.Manager;

public class NamespaceJunction : IManaged, IUse
{
    public List<IManaged> Uses { get; }

    public NamespaceJunction(IManaged ns)
    {
        Uses = ns is IUse use ? use.Uses.ToList() : new List<IManaged>();
    }
}
