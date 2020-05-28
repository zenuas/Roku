using Extensions;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class NamespaceJunction : INamespace, IUse
    {
        public List<IFunctionBody> Functions { get; }
        public List<IStructBody> Structs { get; }
        public List<INamespace> Uses { get; }

        public NamespaceJunction(INamespace ns)
        {
            Functions = ns.Functions;
            Structs = ns.Structs;
            Uses = ns is IUse use ? use.Uses.ToList() : new List<INamespace>();
        }
    }
}
