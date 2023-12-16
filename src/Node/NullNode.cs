﻿using Roku.Parser;

namespace Roku.Node;

public class NullNode : INode, IEvaluableNode
{
    public Symbols Symbol { get; init; } = Symbols.NULL;
    INode IToken<INode>.Value { get => this; }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
}
