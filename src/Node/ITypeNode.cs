namespace Roku.Node;

public interface ITypeNode : INode, IEvaluableNode
{
    public string Name { get; }
}
