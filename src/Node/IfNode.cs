using Roku.Parser;
using System;
using System.Collections.Generic;

namespace Roku.Node;

public class IfNode : INode, IStatementNode, IIfNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public IEvaluableNode Condition { get; }
    public IScopeNode Then { get; }
    public List<IIfNode> ElseIf { get; } = new List<IIfNode>();
    public IScopeNode? Else { get; set; } = null;

    public IfNode(IEvaluableNode cond, IScopeNode then)
    {
        Condition = cond;
        Then = then;
    }
}
