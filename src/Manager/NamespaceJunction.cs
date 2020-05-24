using System.Collections.Generic;

namespace Roku.Manager
{
    public class NamespaceJunction : INamespace, IUse
    {
        public INamespace? Parent { get; set; }
        public List<IFunctionBody> Functions { get; }
        public List<IStructBody> Structs { get; }
        public List<INamespace> Uses { get; }

        public NamespaceJunction(INamespace ns)
        {
            Functions = ns.Functions;
            Structs = ns.Structs;
            Uses = ns is IUse use ? use.Uses : new List<INamespace>();
        }
    }
}
