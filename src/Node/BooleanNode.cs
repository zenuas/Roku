using Roku.Parser;
using System;

namespace Roku.Node;

public class BooleanNode : INode, IEvaluableNode
{
    public Symbols Symbol { get; init; } = Symbols.@bool;
    INode IToken<INode>.Value { get => this; }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public bool Value { get; set; } = false;
}
