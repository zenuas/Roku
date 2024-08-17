using Roku.Parser;
using System.Collections.Generic;

namespace Roku.Node;

public class ClassNode : INode
{
    public Symbols Symbol { get; init; }
    INode IToken<INode>.Value => this;
    public int Indent { get; set; }
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public VariableNode Name { get; set; } = new();
    public List<ITypeNode> Generics { get; init; } = [];
    public List<FunctionNode> Functions { get; init; } = [];
}
