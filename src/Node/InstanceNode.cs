﻿using Roku.Parser;
using System.Collections.Generic;

namespace Roku.Node;

public class InstanceNode : INode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public required SpecializationNode Specialization { get; init; }
    public required List<InstanceMapNode> InstanceMap { get; init; }
}
