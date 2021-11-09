using System;
using System.Collections.Generic;

namespace Roku.Node;

public class LambdaExpressionNode : INode, IEvaluableNode, IScopeNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public ITypeNode? Return { get; set; }
    public List<IDeclareNode> Arguments { get; } = new List<IDeclareNode>();
    public bool IsImplicit { get; set; } = true;
    public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
    public List<FunctionNode> Functions { get; } = new List<FunctionNode>();
    public List<StructNode> Structs { get; } = new List<StructNode>();
    public List<ClassNode> Classes { get; } = new List<ClassNode>();
    public List<InstanceNode> Instances => throw new NotImplementedException();
}
