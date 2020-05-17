using System.Collections.Generic;

namespace Roku.Node
{
    public class TypeGenericsNode : INode, ITypeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public string Name { get; set; } = "";
        public List<ITypeNode> Generics { get; } = new List<ITypeNode>();
    }
}
