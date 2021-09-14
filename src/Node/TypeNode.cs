using System.Collections.Generic;

namespace Roku.Node
{
    public class TypeNode : INode, ITypeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public List<string> Namespace { get; } = new List<string>();
        public string Name { get; set; } = "";
    }
}
