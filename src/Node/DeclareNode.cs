using Roku.Parser;

namespace Roku.Node;

public class DeclareNode : INode, IDeclareNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public required VariableNode Name { get; init; }
    public required ITypeNode Type { get; init; }
}
