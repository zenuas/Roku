using Roku.Parser;
using System;

namespace Roku.Node;

public class LetTypeNode : INode, IStatementNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Var { get; }
    public ITypeNode Type { get; }

    public LetTypeNode(VariableNode v, ITypeNode t)
    {
        Var = v;
        Type = t;
    }
}
