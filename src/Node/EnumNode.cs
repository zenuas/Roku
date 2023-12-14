﻿using Extensions;
using Roku.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Node;

public class EnumNode(ListNode<ITypeNode> types) : INode, ITypeNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public List<ITypeNode> Types { get; } = types.List;
    public string Name => $"[{Types.Select(x => x.Name).Join(" | ")}]";
}
