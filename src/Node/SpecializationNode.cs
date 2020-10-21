using System;
using System.Collections.Generic;

namespace Roku.Node
{
    public class SpecializationNode : INode, ITypeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public IEvaluableNode Expression { get; set; }
        public List<ITypeNode> Generics { get; } = new List<ITypeNode>();
        public string Name => throw new NotImplementedException();

        public SpecializationNode(IEvaluableNode expr)
        {
            Expression = expr;
        }
    }
}
