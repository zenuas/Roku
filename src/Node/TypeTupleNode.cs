using Extensions;
using Roku.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Node;

public class TypeTupleNode : INode, ITypeNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public required List<ITypeNode> Types { get; init; }
    public string Name => $"[{Types.Select(x => x.Name).Join(", ")}]";
}
