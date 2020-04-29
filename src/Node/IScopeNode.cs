using System.Collections.Generic;

namespace Roku.Node
{
    public interface IScopeNode : INode
    {
        public List<IStatementNode> Statements { get; }
        public List<FunctionNode> Functions { get; }
        public bool InnerScope { get; set; }
    }
}
