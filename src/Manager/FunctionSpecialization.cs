namespace Roku.Manager;

public class FunctionSpecialization : IGenericsMapper
{
    public required IFunctionName Body { get; init; }
    public required GenericsMapper GenericsMapper { get; init; }
}
