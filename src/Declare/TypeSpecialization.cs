using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Declare;

public class TypeSpecialization : ITypeDefinition
{
    public string Name { get => Type.ToString()!; }
    public IEvaluable Type { get; }
    public List<ITypeDefinition> Generics { get; } = [];

    public TypeSpecialization(IEvaluable type)
    {
        Type = type;
    }

    public override string ToString() => $"{Type}<{Generics.Select(x => x.ToString()!).Join(", ")}>";
}
