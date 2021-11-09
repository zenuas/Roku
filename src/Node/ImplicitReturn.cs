namespace Roku.Node;

public class ImplicitReturn : INode, IStatementNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public IEvaluableNode Expression { get; set; }

    public ImplicitReturn(IEvaluableNode e)
    {
        Expression = e;
    }
}
