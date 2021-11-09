namespace Roku.Node;

public class NullNode : INode, IEvaluableNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
}
