using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager;

public class StructBody : IStructBody, ILexicalScope, ISpecialization
{
    public required string Name { get; init; }
    public Dictionary<string, IEvaluable> Members { get; init; } = [];
    public List<IOperand> Body { get; init; } = [];
    public required IManaged Namespace { get; init; }
    public ILexicalScope? Parent { get; init; } = null;
    public Dictionary<string, IEvaluable> LexicalScope { get; init; } = [];
    public int MaxTemporaryValue { get; set; } = 0;
    public List<TypeGenericsParameter> Generics { get; init; } = [];
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; init; } = [];
    public StructBodyTypes Type { get; init; } = StructBodyTypes.Struct;
    public Dictionary<ClassBody, VTable> VTables = [];

    public override string ToString() => Name;
}
