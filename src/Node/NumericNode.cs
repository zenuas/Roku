using Roku.Parser;

namespace Roku.Node;

public class NumericNode : INode, IEvaluableNode
{
    public Symbols Symbol { get; init; } = Symbols.NUM;
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public uint Value { get; init; } = 0u;
    public string Format { get; init; } = "";
}
