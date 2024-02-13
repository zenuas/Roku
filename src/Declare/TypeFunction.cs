using Mina.Extension;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Declare;

public class TypeFunction : ITypeDefinition
{
    public string Name => ToString();
    public List<ITypeDefinition> Arguments { get; init; } = [];
    public ITypeDefinition? Return { get; set; } = null;

    public override string ToString() => $"{{{Arguments.Select(x => x.Name).Join(", ")}{(Return is { } r ? $" => {r}" : "")}}}";
}
