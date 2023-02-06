using Roku.Parser;
using System;

namespace Roku.Node;

public class NumericNode : INode, IEvaluableNode
{
    public Symbols Symbol { get; init; } = Symbols.NUM;
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public uint Value { get; init; } = 0u;
    public string Format { get; init; } = "";
}
