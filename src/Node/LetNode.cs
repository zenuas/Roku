﻿using Roku.Parser;

namespace Roku.Node;

public class LetNode : INode, IStatementNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public required VariableNode Var { get; init; }
    public required IEvaluableNode Expression { get; init; }
}
