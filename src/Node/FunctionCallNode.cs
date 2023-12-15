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
    public required IEvaluableNode Expression { get; init; }
    public List<IEvaluableNode> Arguments { get; } = [];
}
