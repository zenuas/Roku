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
    public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
    public List<FunctionNode> Functions { get; } = new List<FunctionNode>();
    public List<StructNode> Structs { get; } = new List<StructNode>();
    public List<ClassNode> Classes { get; } = new List<ClassNode>();
    public List<InstanceNode> Instances => throw new NotImplementedException();
}
