using System;

namespace Roku.Declare;

public class TypeInfoValue : ITypeDefinition
{
    public string Name => ToString();
    public required Type Type { get; init; }

    public override string ToString() => Type.Name;
}
