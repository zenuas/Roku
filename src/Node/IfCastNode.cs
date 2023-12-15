using Roku.Parser;
using System;
using System.Collections.Generic;

namespace Roku.Node;

public class IfCastNode : INode, IStatementNode, IIfNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public required VariableNode Name { get; init; }
    public required ITypeNode Declare { get; init; }
    public required IEvaluableNode Condition { get; init; }
    public required IScopeNode Then { get; init; }
    public List<IIfNode> ElseIf { get; init; } = [];
    public IScopeNode? Else { get; set; } = null;
}
