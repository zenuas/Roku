using System.Collections.Generic;

namespace Roku.Manager
{
    public interface INamespaceBody : INamespace
    {
        public List<IFunctionName> Functions { get; }
        public List<IStructBody> Structs { get; }
        public List<ClassBody> Classes { get; }
        public List<InstanceBody> Instances { get; }
    }
}
