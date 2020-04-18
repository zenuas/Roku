using System.Collections.Generic;

namespace Roku.Manager
{
    public class SourceCodeBody : INamespace
    {
        public List<IFunctionBody> Functions { get; } = new List<IFunctionBody>();
        public List<StructBody> Structs { get; } = new List<StructBody>();
        public List<INamespace> Uses { get; } = new List<INamespace>();
    }
}
