using Roku.Parser;
using System;
using System.Collections.Generic;

namespace Roku.Node;

public class IfArrayCastNode(ListNode<VariableNode> array_pattern, IEvaluableNode cond, IScopeNode then) : INode, IStatementNode, IIfNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public ListNode<VariableNode> ArrayPattern { get; set; } = array_pattern;
    public IEvaluableNode Condition { get; } = cond;
    public IScopeNode Then { get; } = then;
    public List<IIfNode> ElseIf { get; } = [];
    public IScopeNode? Else { get; set; } = null;
}
