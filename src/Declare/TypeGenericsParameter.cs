namespace Roku.Declare;

public class TypeGenericsParameter : ITypeDefinition
{
    public string Name { get; }

    public TypeGenericsParameter(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;
}
