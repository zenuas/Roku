using Roku.Parser;
using System;
using System.Collections.Generic;

namespace Roku.Node;

public class StructNode : INode, IScopeNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Name { get; set; } = new();
    public List<ITypeNode> Generics { get; init; } = [];
    public List<IStatementNode> Statements { get; init; } = [];
    public List<FunctionNode> Functions => throw new NotImplementedException();
    public List<StructNode> Structs => throw new NotImplementedException();
    public List<ClassNode> Classes => throw new NotImplementedException();
    public List<InstanceNode> Instances => throw new NotImplementedException();
}
