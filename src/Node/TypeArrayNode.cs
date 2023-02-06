using Roku.Parser;
using System;

namespace Roku.Node;

public class TypeArrayNode : INode, ITypeNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public ITypeNode Item { get; }
    public string Name => $"[{Item.Name}]";

    public TypeArrayNode(ITypeNode item)
    {
        Item = item;
    }
}
