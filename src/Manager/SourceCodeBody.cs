using System.Collections.Generic;

namespace Roku.Manager
{
    public class SourceCodeBody : INamespace
    {
        public INamespace? Parent { get; }
        public List<IFunctionBody> Functions { get; } = new List<IFunctionBody>();
        public List<IStructBody> Structs { get; } = new List<IStructBody>();
        public List<INamespace> Uses { get; } = new List<INamespace>();

        public SourceCodeBody(INamespace ns)
        {
            Parent = ns;
        }
    }
}
