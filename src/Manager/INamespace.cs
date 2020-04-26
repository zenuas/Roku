using System.Collections.Generic;

namespace Roku.Manager
{
    public interface INamespace
    {
        public INamespace? Parent { get; }
        public List<IFunctionBody> Functions { get; }
        public List<IStructBody> Structs { get; }
    }
}
