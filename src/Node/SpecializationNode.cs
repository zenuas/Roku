﻿using Mina.Extension;
using Roku.Parser;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Node;

public class SpecializationNode : INode, ITypeNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public required IEvaluableNode Expression { get; init; }
    public List<ITypeNode> Generics { get; init; } = [];
    public string Name => NodeIterator.PropertyToList(Expression).Select(x => x is VariableNode v ? v.Name : x is TypeNode t ? t.Name : throw new()).Join(".");
}
