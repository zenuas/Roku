using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Declare;

public class TypeSpecialization : ITypeDefinition
{
    public string Name => Type.ToString()!;
    public required IEvaluable Type { get; init; }
    public List<ITypeDefinition> Generics { get; } = [];

    public override string ToString() => $"{Type}<{Generics.Select(x => x.ToString()!).Join(", ")}>";
}
