﻿using Roku.Parser;
using System.Collections.Generic;

namespace Roku.Node;

public class LetTupleAssignmentNode : INode, IStatementNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public List<ITupleBind> Assignment { get; init; } = [];
    public required IEvaluableNode Expression { get; init; }
}
