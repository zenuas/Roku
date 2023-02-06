using Roku.Parser;
using System;
using System.Collections.Generic;

namespace Roku.Node;

public class ClassNode : INode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Name { get; set; } = new VariableNode();
    public List<ITypeNode> Generics { get; } = new List<ITypeNode>();
    public List<FunctionNode> Functions { get; } = new List<FunctionNode>();
}
