using Roku.Parser;
using System;

namespace Roku.Node;

public class PropertyNode : INode, IEvaluableNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public IEvaluableNode Left { get; }
    public VariableNode Right { get; }

    public PropertyNode(IEvaluableNode left, VariableNode right)
    {
        Left = left;
        Right = right;
    }
}
