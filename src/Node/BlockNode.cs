using System.Collections.Generic;

namespace Roku.Node
{
    public class BlockNode : Node, IScopeNode
    {
        public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
        public List<FunctionNode> Functions { get; } = new List<FunctionNode>();
        public bool InnerScope { get; set; } = true;
    }
}
