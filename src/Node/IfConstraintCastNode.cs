using System.Collections.Generic;

namespace Roku.Node
{
    public class IfConstraintCastNode : INode, IStatementNode, IIfNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public VariableNode Name { get; set; }
        public ListNode<SpecializationNode> Specializations { get; set; }
        public VariableNode Generic { get; set; }
        public IEvaluableNode Condition { get; set; }
        public IScopeNode Then { get; set; }
        public List<IIfNode> ElseIf { get; } = new List<IIfNode>();
        public IScopeNode? Else { get; set; } = null;

        public IfConstraintCastNode(VariableNode name, ListNode<SpecializationNode> specials, VariableNode gene, IEvaluableNode cond, IScopeNode then)
        {
            Name = name;
            Specializations = specials;
            Generic = gene;
            Condition = cond;
            Then = then;
        }
    }
}
