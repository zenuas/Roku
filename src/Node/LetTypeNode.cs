namespace Roku.Node;

public class LetTypeNode : INode, IStatementNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Var { get; }
    public ITypeNode Type { get; }

    public LetTypeNode(VariableNode v, ITypeNode t)
    {
        Var = v;
        Type = t;
    }
}
