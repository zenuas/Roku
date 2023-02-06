using Roku.Parser;
using System;

namespace Roku.Node;

public class VariableNode : INode, IEvaluableNode
{
    public Symbols Symbol { get; init; } = Symbols.VAR;
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public string Name { get; init; } = "";
}
