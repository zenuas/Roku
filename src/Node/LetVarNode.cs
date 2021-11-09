namespace Roku.Node;

public class LetVarNode : INode, ITupleBind
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Var { get; }

    public LetVarNode(VariableNode v)
    {
        Var = v;
    }
}
