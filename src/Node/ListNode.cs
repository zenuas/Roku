﻿using Roku.Parser;
using System.Collections.Generic;

namespace Roku.Node;

public class ListNode<T> : INode, IEvaluableNode
    where T : INode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public List<T> List { get; init; } = [];
}
