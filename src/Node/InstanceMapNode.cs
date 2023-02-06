using Roku.Parser;
using System;
using System.Collections.Generic;

namespace Roku.Node;

public class InstanceMapNode : INode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Name { get; set; } = new VariableNode();
    public List<IDeclareNode> Arguments { get; } = new List<IDeclareNode>();
    public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
}
