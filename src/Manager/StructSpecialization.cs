namespace Roku.Manager;

public class StructSpecialization : IStructBody, IManaged, IGenericsMapper
{
    public string Name => Body.Name;
    public required IStructBody Body { get; init; }
    public required GenericsMapper GenericsMapper { get; init; }
}
