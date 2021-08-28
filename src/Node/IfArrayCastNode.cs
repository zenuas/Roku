using System.Collections.Generic;

namespace Roku.Node
{
    public class IfArrayCastNode : INode, IStatementNode, IIfNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public ListNode<VariableNode> ArrayPattern { get; set; }
        public IEvaluableNode Condition { get; set; }
        public IScopeNode Then { get; set; }
        public List<IIfNode> ElseIf { get; } = new List<IIfNode>();
        public IScopeNode? Else { get; set; } = null;

        public IfArrayCastNode(ListNode<VariableNode> array_pattern, IEvaluableNode cond, IScopeNode then)
        {
            ArrayPattern = array_pattern;
            Condition = cond;
            Then = then;
        }
    }
}
