namespace Roku.Manager;

public class FunctionMapper : IStructBody
{
    public IFunctionName Function { get; }
    public string Name { get => Function.Name; }
    public TypeMapper TypeMapper { get; init; } = [];

    public FunctionMapper(IFunctionName f)
    {
        Function = f;
    }
}
