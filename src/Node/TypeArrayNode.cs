﻿using Roku.Parser;

namespace Roku.Node;

public class TypeArrayNode : INode, ITypeNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public required ITypeNode Item { get; init; }
    public string Name => $"[{Item.Name}]";
}
