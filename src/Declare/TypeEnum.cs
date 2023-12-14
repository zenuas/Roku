using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Declare;

public class TypeEnum : ITypeDefinition
{
    public string Name { get => ToString(); }
    public List<ITypeDefinition> Enums { get; } = [];

    public TypeEnum(IEnumerable<ITypeDefinition> enums)
    {
        Enums.AddRange(enums);
    }

    public override string ToString() => $"[{Enums.Select(x => x.ToString()!).Join(" | ")}]";
}
