using Mina.Extension;
using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Manager;

public class FunctionBody : IFunctionBody, ILexicalScope, IConstraints
{
    public required string Name { get; init; }
    public ITypeDefinition? Return { get; set; } = null;
    public List<(VariableValue Name, ITypeDefinition Type)> Arguments { get; init; } = [];
    public List<IOperand> Body { get; init; } = [];
    public required IManaged Namespace { get; init; }
    public ILexicalScope? Parent { get; init; }
    public Dictionary<string, IEvaluable> LexicalScope { get; init; } = [];
    public int MaxTemporaryValue { get; set; } = 0;
    public List<TypeGenericsParameter> Generics { get; init; } = [];
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; init; } = [];
    public List<(VariableValue Class, List<ITypeDefinition> Generics)> Constraints { get; init; } = [];
    public Dictionary<VariableValue, ILexicalScope> Capture { get; init; } = [];

    public override string ToString() => $"sub {Name}({Arguments.Select(x => x.Name.ToString() + " : " + x.Type.ToString()).Join(", ")}){(Return is null ? "" : " " + Return.ToString())}";
}
