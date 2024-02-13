using Mina.Extension;
using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Manager;

public class EnumStructBody : IStructBody, ILexicalScope, ISpecialization
{
    public string Name => ToString();
    public List<IStructBody> Enums { get; init; } = [];
    public List<IOperand> Body { get; init; } = [];
    public required IManaged Namespace { get; init; }
    public ILexicalScope? Parent { get; init; } = null;
    public Dictionary<string, IEvaluable> LexicalScope { get; init; } = [];
    public int MaxTemporaryValue { get; set; } = 0;
    public List<TypeGenericsParameter> Generics { get; init; } = [];
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; init; } = [];

    public override string ToString() => $"[{Enums.Select(x => x.ToString()!).Join(" | ")}]";
}
