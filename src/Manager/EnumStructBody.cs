using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Manager;

public class EnumStructBody : IStructBody, ILexicalScope, ISpecialization
{
    public string Name { get => ToString(); }
    public List<IStructBody> Enums { get; } = new List<IStructBody>();
    public List<IOperand> Body { get; } = new List<IOperand>();
    public IManaged Namespace { get; }
    public ILexicalScope? Parent { get; } = null;
    public Dictionary<string, IEvaluable> LexicalScope { get; } = new Dictionary<string, IEvaluable>();
    public int MaxTemporaryValue { get; set; } = 0;
    public List<TypeGenericsParameter> Generics { get; } = new List<TypeGenericsParameter>();
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = new Dictionary<GenericsMapper, TypeMapper>();

    public EnumStructBody(IManaged ns)
    {
        Namespace = ns;
    }

    public override string ToString() => $"[{Enums.Select(x => x.ToString()!).Join(" | ")}]";
}
