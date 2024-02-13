using Mina.Extension;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Declare;

public class TypeEnum : ITypeDefinition
{
    public string Name => ToString();
    public List<ITypeDefinition> Enums { get; init; } = [];

    public TypeEnum(IEnumerable<ITypeDefinition> enums)
    {
        Enums.AddRange(enums);
    }

    public override string ToString() => $"[{Enums.Select(x => x.ToString()!).Join(" | ")}]";
}
