namespace Roku.Node;

public class ImplicitDeclareNode : INode, IDeclareNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Name { get; set; }

    public ImplicitDeclareNode(VariableNode name)
    {
        Name = name;
    }
}
