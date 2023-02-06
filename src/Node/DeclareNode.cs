using Roku.Parser;
using System;

namespace Roku.Node;

public class DeclareNode : INode, IDeclareNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Name { get; }
    public ITypeNode Type { get; }

    public DeclareNode(VariableNode name, ITypeNode type)
    {
        Name = name;
        Type = type;
    }
}
