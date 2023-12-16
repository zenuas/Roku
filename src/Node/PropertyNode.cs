using Roku.Parser;

namespace Roku.Node;

public class PropertyNode : INode, IEvaluableNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public required IEvaluableNode Left { get; init; }
    public required VariableNode Right { get; init; }
}
