using Roku.Parser;
using System;
using System.Collections.Generic;

namespace Roku.Node;

public class FunctionNode : INode, IScopeNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Name { get; set; } = new();
    public ITypeNode? Return { get; set; }
    public List<DeclareNode> Arguments { get; init; } = [];
    public List<IStatementNode> Statements { get; init; } = [];
    public List<SpecializationNode> Constraints { get; init; } = [];
    public List<FunctionNode> Functions { get; init; } = [];
    public List<StructNode> Structs { get; init; } = [];
    public List<ClassNode> Classes { get; init; } = [];
    public List<InstanceNode> Instances => throw new NotImplementedException();
}
