using Roku.Parser;

namespace Roku.Node;

public class LetVarNode : INode, ITupleBind
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public required VariableNode Var { get; init; }
}
