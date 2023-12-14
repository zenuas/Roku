using Roku.Parser;
using System;
using System.Collections.Generic;

namespace Roku.Node;

public class FunctionCallNode : INode, IEvaluableNode, IStatementNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public IEvaluableNode Expression { get; set; }
    public List<IEvaluableNode> Arguments { get; } = [];

    public FunctionCallNode(IEvaluableNode expr)
    {
        Expression = expr;
    }
}
