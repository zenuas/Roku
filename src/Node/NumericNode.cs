namespace Roku.Node;

public class NumericNode : INode, IEvaluableNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public uint Value { get; init; } = 0u;
    public string Format { get; init; } = "";
}
