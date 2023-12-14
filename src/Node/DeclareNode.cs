using Roku.Parser;
using System;

namespace Roku.Node;

public class DeclareNode(VariableNode name, ITypeNode type) : INode, IDeclareNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Name { get; } = name;
    public ITypeNode Type { get; } = type;
}
