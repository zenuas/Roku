using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Node
{
    public class SpecializationNode : INode, ITypeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public IEvaluableNode Expression { get; set; }
        public List<ITypeNode> Generics { get; } = new List<ITypeNode>();
        public string Name => NodeIterator.PropertyToList(Expression).Select(x => x is VariableNode v ? v.Name : x is TypeNode t ? t.Name : throw new Exception()).Join(".");

        public SpecializationNode(IEvaluableNode expr)
        {
            Expression = expr;
        }
    }
}
