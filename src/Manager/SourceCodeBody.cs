using System.Collections.Generic;

namespace Roku.Manager
{
    public class SourceCodeBody : INamespaceBody, IUse
    {
        public List<IFunctionName> Functions { get; } = new List<IFunctionName>();
        public List<IStructBody> Structs { get; } = new List<IStructBody>();
        public List<INamespace> Uses { get; } = new List<INamespace>();
    }
}
