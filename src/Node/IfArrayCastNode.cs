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
    public ListNode<VariableNode> ArrayPattern { get; set; }
    public IEvaluableNode Condition { get; }
    public IScopeNode Then { get; }
    public List<IIfNode> ElseIf { get; } = [];
    public IScopeNode? Else { get; set; } = null;

    public IfArrayCastNode(ListNode<VariableNode> array_pattern, IEvaluableNode cond, IScopeNode then)
    {
        ArrayPattern = array_pattern;
        Condition = cond;
        Then = then;
    }
}
