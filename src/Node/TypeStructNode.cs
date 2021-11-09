using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Node;

public class TypeStructNode : INode, ITypeNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode StructName { get; set; } = new VariableNode();
    public List<DeclareNode> Arguments { get; } = new List<DeclareNode>();
    public string Name => $"struct {StructName.Name}({Arguments.Select(x => $"{x.Name.Name}: {x.Type.Name}").Join(", ")})";
}
