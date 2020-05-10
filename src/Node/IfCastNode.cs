using System.Collections.Generic;

namespace Roku.Node
{
    public class IfCastNode : INode, IStatementNode, IIfNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public VariableNode Name { get; set; }
        public TypeNode Declare { get; set; }
        public IEvaluableNode Condition { get; set; }
        public IScopeNode Then { get; set; }
        public List<IIfNode> ElseIf { get; } = new List<IIfNode>();
        public IScopeNode? Else { get; set; } = null;

        public IfCastNode(VariableNode name, TypeNode declare, IEvaluableNode cond, IScopeNode then)
        {
            Name = name;
            Declare = declare;
            Condition = cond;
            Then = then;
        }
    }
}
