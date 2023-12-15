using Roku.Parser;
using System;
using System.Collections.Generic;

namespace Roku.Node;

public class IfArrayCastNode : INode, IStatementNode, IIfNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public required ListNode<VariableNode> ArrayPattern { get; init; }
    public required IEvaluableNode Condition { get; init; }
    public required IScopeNode Then { get; init; }
    public List<IIfNode> ElseIf { get; } = [];
    public IScopeNode? Else { get; set; } = null;
}
