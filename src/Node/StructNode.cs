using System.Collections.Generic;

namespace Roku.Node
{
    public class StructNode : INode, IScopeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public VariableNode Name { get; set; } = new VariableNode();
        public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
        public List<FunctionNode> Functions => throw new System.NotImplementedException();
    }
}
