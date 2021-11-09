namespace Roku.Node;

public class LetNode : INode, IStatementNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Var { get; }
    public IEvaluableNode Expression { get; set; }

    public LetNode(VariableNode v, IEvaluableNode e)
    {
        Var = v;
        Expression = e;
    }
}
