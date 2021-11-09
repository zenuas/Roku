namespace Roku.Node;

public class VariableNode : INode, IEvaluableNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public string Name { get; init; } = "";
}
