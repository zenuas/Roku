namespace Roku.Manager;

public class FunctionSpecialization(IFunctionName body, GenericsMapper gen_map) : IGenericsMapper
{
    public IFunctionName Body { get; } = body;
    public GenericsMapper GenericsMapper { get; } = gen_map;
}
