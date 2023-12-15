using Roku.Parser;
using System;
using System.Collections.Generic;

namespace Roku.Node;

public class ProgramNode : INode, IScopeNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; init => throw new NotImplementedException(); }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public List<VariableNode> Uses { get; init; } = [];
    public string FileName { get; init; } = "";
    public List<IStatementNode> Statements { get; init; } = [];
    public List<FunctionNode> Functions { get; init; } = [];
    public List<StructNode> Structs { get; init; } = [];
    public List<ClassNode> Classes { get; init; } = [];
    public List<InstanceNode> Instances { get; init; } = [];
}
