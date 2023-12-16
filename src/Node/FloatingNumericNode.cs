using Roku.Parser;

namespace Roku.Node;

public class FloatingNumericNode : INode, IEvaluableNode, IToken<INode>
{
    public Symbols Symbol { get; init; } = Symbols.FLOAT;
    INode IToken<INode>.Value { get => this; }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public double Value { get; set; } = 0.0;
    public string Format { get; set; } = "";
}
