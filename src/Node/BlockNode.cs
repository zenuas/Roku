using Roku.Parser;
using System;
using System.Collections.Generic;

namespace Roku.Node;

public class BlockNode : INode, IScopeNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public List<IStatementNode> Statements { get; init; } = [];
    public List<FunctionNode> Functions { get; init; } = [];
    public List<StructNode> Structs { get; init; } = [];
    public List<ClassNode> Classes { get; init; } = [];
    public List<InstanceNode> Instances => throw new NotImplementedException();
}
