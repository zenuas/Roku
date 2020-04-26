using System.Collections.Generic;

namespace Roku.Node
{
    public class FunctionCallNode : Node, IEvaluableNode, IStatementNode
    {
        public IEvaluableNode Expression { get; set; }
        public List<IEvaluableNode> Arguments { get; } = new List<IEvaluableNode>();

        public FunctionCallNode(IEvaluableNode expr)
        {
            Expression = expr;
        }
    }
}
