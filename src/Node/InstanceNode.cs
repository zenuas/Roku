using Roku.Parser;
using System;
using System.Collections.Generic;

namespace Roku.Node;

public class InstanceNode(SpecializationNode spec, ListNode<InstanceMapNode> maps) : INode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public SpecializationNode Specialization { get; } = spec;
    public List<InstanceMapNode> InstanceMap { get; } = maps.List;
}
