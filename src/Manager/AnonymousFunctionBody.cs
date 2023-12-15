using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager;

public class AnonymousFunctionBody : IFunctionBody, IStructBody, ILexicalScope
{
    public required string Name { get; init; }
    public ITypeDefinition? Return { get; set; } = null;
    public List<(VariableValue Name, ITypeDefinition Type)> Arguments { get; init; } = [];
    public bool IsImplicit { get; set; } = true;
    public List<IOperand> Body { get; init; } = [];
    public required IManaged Namespace { get; init; }
    public required ILexicalScope? Parent { get; set; }
    public Dictionary<string, IEvaluable> LexicalScope { get; init; } = [];
    public int MaxTemporaryValue { get; set; } = 0;
    public List<TypeGenericsParameter> Generics { get; init; } = [];
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; init; } = [];
}
