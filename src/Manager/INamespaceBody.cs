using System.Collections.Generic;

namespace Roku.Manager
{
    public interface INamespaceBody : INamespace
    {
        public List<IFunctionBody> Functions { get; }
        public List<IStructBody> Structs { get; }
    }
}
