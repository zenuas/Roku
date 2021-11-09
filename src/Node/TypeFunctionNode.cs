using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Node;

public class TypeFunctionNode : INode, ITypeNode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public List<ITypeNode> Arguments { get; } = new List<ITypeNode>();
    public ITypeNode? Return { get; set; } = null;
    public string Name => $"{{{Arguments.Select(x => x.Name).Join(", ")}{(Return is { } r ? $" => {r.Name}" : "")}}}";
}
