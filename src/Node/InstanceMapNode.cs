using Roku.Parser;
using System.Collections.Generic;

namespace Roku.Node;

public class InstanceMapNode : INode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Name { get; set; } = new();
    public List<IDeclareNode> Arguments { get; init; } = [];
    public List<IStatementNode> Statements { get; init; } = [];
}
