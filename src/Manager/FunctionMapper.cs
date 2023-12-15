namespace Roku.Manager;

public class FunctionMapper : IStructBody
{
    public required IFunctionName Function { get; init; }
    public string Name => Function.Name;
    public TypeMapper TypeMapper { get; init; } = [];
}
