using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager;

public class StructBody : IStructBody, ILexicalScope, ISpecialization
{
    public string Name { get; }
    public Dictionary<string, IEvaluable> Members { get; } = [];
    public List<IOperand> Body { get; } = [];
    public IManaged Namespace { get; }
    public ILexicalScope? Parent { get; } = null;
    public Dictionary<string, IEvaluable> LexicalScope { get; } = [];
    public int MaxTemporaryValue { get; set; } = 0;
    public List<TypeGenericsParameter> Generics { get; } = [];
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = [];
    public StructBodyTypes Type { get; init; } = StructBodyTypes.Struct;
    public Dictionary<ClassBody, VTable> VTables = [];

    public StructBody(IManaged ns, string name)
    {
        Namespace = ns;
        Name = name;
    }

    public override string ToString() => Name;
}
