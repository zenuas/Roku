using System.Collections.Generic;

namespace Roku.Declare;

public class TypeValue : ITypeDefinition
{
    public List<string> Namespace { get; } = new List<string>();
    public string Name { get; init; } = "";

    public override string ToString() => Name;
}
