using System.Collections.Generic;

namespace Roku.Node
{
    public class InstanceMapNode : INode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public VariableNode Name { get; set; } = new VariableNode();
        public List<IDeclareNode> Arguments { get; } = new List<IDeclareNode>();
        public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
    }
}
