using Roku.Parser;
using System;

namespace Roku.Node;

public class ImplicitReturn : INode, IStatementNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public IEvaluableNode Expression { get; }

    public ImplicitReturn(IEvaluableNode e)
    {
        Expression = e;
    }
}
