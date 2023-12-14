namespace Roku.Manager;

public class StructSpecialization(IStructBody body, GenericsMapper gen_map) : IStructBody, IManaged, IGenericsMapper
{
    public string Name { get => Body.Name; }
    public IStructBody Body { get; } = body;
    public GenericsMapper GenericsMapper { get; } = gen_map;
}
