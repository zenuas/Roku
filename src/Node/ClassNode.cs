using System.Collections.Generic;

namespace Roku.Node
{
    public class ClassNode : INode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public VariableNode Name { get; set; } = new VariableNode();
        public List<ITypeNode> Generics { get; } = new List<ITypeNode>();
        public List<FunctionNode> Functions { get; } = new List<FunctionNode>();
    }
}
