namespace Roku.Node;

public class DeclareNode : INode, IDeclareNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Name { get; }
    public ITypeNode Type { get; }

    public DeclareNode(VariableNode name, ITypeNode type)
    {
        Name = name;
        Type = type;
    }
}
