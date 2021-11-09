using System.Collections.Generic;

namespace Roku.Node;

public class ProgramNode : INode, IScopeNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public List<VariableNode> Uses { get; } = new List<VariableNode>();
    public string FileName { get; init; } = "";
    public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
    public List<FunctionNode> Functions { get; } = new List<FunctionNode>();
    public List<StructNode> Structs { get; } = new List<StructNode>();
    public List<ClassNode> Classes { get; } = new List<ClassNode>();
    public List<InstanceNode> Instances { get; } = new List<InstanceNode>();
}
