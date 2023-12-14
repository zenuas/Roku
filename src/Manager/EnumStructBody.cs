using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Manager;

public class EnumStructBody(IManaged ns) : IStructBody, ILexicalScope, ISpecialization
{
    public string Name { get => ToString(); }
    public List<IStructBody> Enums { get; } = [];
    public List<IOperand> Body { get; } = [];
    public IManaged Namespace { get; } = ns;
    public ILexicalScope? Parent { get; } = null;
    public Dictionary<string, IEvaluable> LexicalScope { get; } = [];
    public int MaxTemporaryValue { get; set; } = 0;
    public List<TypeGenericsParameter> Generics { get; } = [];
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = [];

    public override string ToString() => $"[{Enums.Select(x => x.ToString()!).Join(" | ")}]";
}
