using Roku.Parser;

namespace Roku.Node;

public class VariableNode : INode, IEvaluableNode
{
    public Symbols Symbol { get; init; } = Symbols.VAR;
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public string Name { get; init; } = "";
}
