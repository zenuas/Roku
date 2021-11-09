namespace Roku.Node;

public class BooleanNode : INode, IEvaluableNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public bool Value { get; set; } = false;
}
