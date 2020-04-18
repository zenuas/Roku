using System.Collections.Generic;

namespace Roku.Node
{
    public class FunctionNode : Node
    {
        public VariableNode Name { get; }
        public VariableNode? Return { get; set; }
        public List<DeclareNode> Arguments { get; } = new List<DeclareNode>();
        public List<IStatementNode> Statements { get; } = new List<IStatementNode>();

        public FunctionNode(VariableNode name)
        {
            Name = name;
        }
    }
}
