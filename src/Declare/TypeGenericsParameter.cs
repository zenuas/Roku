namespace Roku.Declare;

public class TypeGenericsParameter : ITypeDefinition
{
    public string Name { get; init; } = "";

    public override string ToString() => Name;
}
