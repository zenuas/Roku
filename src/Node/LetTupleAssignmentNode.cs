using System.Collections.Generic;

namespace Roku.Node;

public class LetTupleAssignmentNode : INode, IStatementNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public List<ITupleBind> Assignment { get; } = new List<ITupleBind>();
    public IEvaluableNode Expression { get; }

    public LetTupleAssignmentNode(IEvaluableNode e)
    {
        Expression = e;
    }
}
