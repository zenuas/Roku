using System;
using System.Collections.Generic;

namespace Roku.Node
{
    public class FunctionNode : INode, IScopeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public VariableNode Name { get; set; } = new VariableNode();
        public ITypeNode? Return { get; set; }
        public List<DeclareNode> Arguments { get; } = new List<DeclareNode>();
        public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
        public List<SpecializationNode> Constraints { get; } = new List<SpecializationNode>();
        public List<FunctionNode> Functions { get; } = new List<FunctionNode>();
        public List<StructNode> Structs { get; } = new List<StructNode>();
        public List<ClassNode> Classes { get; } = new List<ClassNode>();
        public List<InstanceNode> Instances => throw new NotImplementedException();
    }
}
