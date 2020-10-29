using Extensions;
using System.Collections.Generic;

namespace Roku.Node
{
    public class TypeStructNode : INode, ITypeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public VariableNode StructName { get; set; } = new VariableNode();
        public List<DeclareNode> Arguments { get; } = new List<DeclareNode>();
        public string Name => $"struct {StructName.Name}({Arguments.Map(x => $"{x.Name.Name}: {x.Type.Name}").Join(", ")})";
    }
}
