using System.Collections.Generic;

namespace Roku.Node
{
    public class FunctionCallNode : INode, IEvaluableNode, IStatementNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public IEvaluableNode Expression { get; set; }
        public List<IEvaluableNode> Arguments { get; } = new List<IEvaluableNode>();

        public FunctionCallNode(IEvaluableNode expr)
        {
            Expression = expr;
        }
    }
}
