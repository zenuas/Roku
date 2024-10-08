﻿using Roku.Parser;
using System.Collections.Generic;

namespace Roku.Node;

public class TypeNode : INode, ITypeNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public List<string> Namespace { get; init; } = [];
    public string Name { get; set; } = "";
}
