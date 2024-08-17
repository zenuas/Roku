using Roku.Parser;

namespace Roku.Node;

public class TokenNode : INode, IEvaluableNode
{
    public required Symbols Symbol { get; init; }
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public string Name { get; set; } = "";
}
