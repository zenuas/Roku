using Roku.Parser;
using System;
using System.Collections.Generic;

namespace Roku.Node;

public class BlockNode : INode, IScopeNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public List<IStatementNode> Statements { get; } = [];
    public List<FunctionNode> Functions { get; } = [];
    public List<StructNode> Structs { get; } = [];
    public List<ClassNode> Classes { get; } = [];
    public List<InstanceNode> Instances => throw new NotImplementedException();
}
