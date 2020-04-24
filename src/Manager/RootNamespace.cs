using System.Collections.Generic;

namespace Roku.Manager
{
    public class RootNamespace : INamespace
    {
        public List<IFunctionBody> Functions { get; } = new List<IFunctionBody>();
        public List<StructBody> Structs { get; } = new List<StructBody>();
    }
}
