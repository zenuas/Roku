using System.Collections.Generic;

namespace Roku.Node
{
    public class StructNode : INode, IScopeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public VariableNode Name { get; set; } = new VariableNode();
        public List<ITypeNode> Generics { get; } = new List<ITypeNode>();
        public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
        public List<FunctionNode> Functions => throw new System.NotImplementedException();
        public List<StructNode> Structs => throw new System.NotImplementedException();
    }
}
