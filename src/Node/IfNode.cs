using System.Collections.Generic;

namespace Roku.Node;

public class IfNode : INode, IStatementNode, IIfNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public IEvaluableNode Condition { get; set; }
    public IScopeNode Then { get; set; }
    public List<IIfNode> ElseIf { get; } = new List<IIfNode>();
    public IScopeNode? Else { get; set; } = null;

    public IfNode(IEvaluableNode cond, IScopeNode then)
    {
        Condition = cond;
        Then = then;
    }
}
