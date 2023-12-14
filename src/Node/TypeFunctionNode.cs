using Extensions;
using Roku.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Node;

public class TypeFunctionNode : INode, ITypeNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public List<ITypeNode> Arguments { get; } = [];
    public ITypeNode? Return { get; set; } = null;
    public string Name => $"{{{Arguments.Select(x => x.Name).Join(", ")}{(Return is { } r ? $" => {r.Name}" : "")}}}";
}
