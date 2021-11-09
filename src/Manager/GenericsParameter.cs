namespace Roku.Manager;

public class GenericsParameter : IStructBody
{
    public string Name { get; init; } = "";

    public override string ToString() => Name;
}
