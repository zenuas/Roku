﻿using Roku.Parser;

namespace Roku.Node;

public class StringNode : INode, IEvaluableNode
{
    public Symbols Symbol { get; init; } = Symbols.STR;
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public string Value { get; set; } = "";
}
