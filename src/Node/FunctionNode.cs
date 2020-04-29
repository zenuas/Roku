using System.Collections.Generic;

namespace Roku.Node
{
    public class FunctionNode : Node, IScopeNode
    {
        public VariableNode Name { get; set; } = new VariableNode();
        public TypeNode? Return { get; set; }
        public List<DeclareNode> Arguments { get; } = new List<DeclareNode>();
        public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
        public List<FunctionNode> Functions { get; } = new List<FunctionNode>();
        public bool InnerScope { get; set; } = false;
    }
}
