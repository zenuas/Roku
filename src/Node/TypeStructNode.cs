using Mina.Extensions;
using Roku.Parser;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Node;

public class TypeStructNode : INode, ITypeNode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value { get => this; }
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode StructName { get; init; } = new();
    public List<DeclareNode> Arguments { get; init; } = [];
    public string Name => $"struct {StructName.Name}({Arguments.Select(x => $"{x.Name.Name}: {x.Type.Name}").Join(", ")})";
}
