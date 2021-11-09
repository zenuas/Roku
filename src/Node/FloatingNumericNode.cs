namespace Roku.Node;

public class FloatingNumericNode : INode, IEvaluableNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public double Value { get; set; } = 0.0;
    public string Format { get; set; } = "";
}
