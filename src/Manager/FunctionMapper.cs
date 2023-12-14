namespace Roku.Manager;

public class FunctionMapper(IFunctionName f) : IStructBody
{
    public IFunctionName Function { get; } = f;
    public string Name { get => Function.Name; }
    public TypeMapper TypeMapper { get; init; } = [];
}
