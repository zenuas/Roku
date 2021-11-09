using System;

namespace Roku.Declare;

public class TypeInfoValue : ITypeDefinition
{
    public string Name { get => ToString(); }
    public Type Type { get; set; }

    public TypeInfoValue(Type type)
    {
        Type = type;
    }

    public override string ToString() => Type.Name;
}
