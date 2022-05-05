namespace Roku.Manager;

public class StructSpecialization : IStructBody, IManaged, IGenericsMapper
{
    public string Name { get => Body.Name; }
    public IStructBody Body { get; }
    public GenericsMapper GenericsMapper { get; }


    public StructSpecialization(IStructBody body, GenericsMapper gen_map)
    {
        Body = body;
        GenericsMapper = gen_map;
    }
}
