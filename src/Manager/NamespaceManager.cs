using System.Collections.Generic;

namespace Roku.Manager
{
    public class NamespaceManager : INamespace
    {
        public NamespaceManager? Parent;
        public string Name { get; set; }
        public List<NamespaceManager> SubNamaspaces { get; } = new List<NamespaceManager>();
        public List<IFunctionBody> Functions { get; } = new List<IFunctionBody>();
        public List<StructBody> Structs { get; } = new List<StructBody>();

        public NamespaceManager(string name)
        {
            Name = name;
        }
    }
}
