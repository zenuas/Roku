using System;

namespace Roku.Declare;

public class TypeInfoValue(Type type) : ITypeDefinition
{
    public string Name { get => ToString(); }
    public Type Type { get; } = type;

    public override string ToString() => Type.Name;
}
