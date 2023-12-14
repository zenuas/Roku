using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Declare;

public class TypeSpecialization(IEvaluable type) : ITypeDefinition
{
    public string Name => Type.ToString()!;
    public IEvaluable Type { get; } = type;
    public List<ITypeDefinition> Generics { get; } = [];

    public override string ToString() => $"{Type}<{Generics.Select(x => x.ToString()!).Join(", ")}>";
}
