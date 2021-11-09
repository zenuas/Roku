namespace Roku.Node;

public class PropertyNode : INode, IEvaluableNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public IEvaluableNode Left { get; }
    public VariableNode Right { get; }

    public PropertyNode(IEvaluableNode left, VariableNode right)
    {
        Left = left;
        Right = right;
    }
}
