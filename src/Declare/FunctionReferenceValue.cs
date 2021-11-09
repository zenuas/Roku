namespace Roku.Declare;

public class FunctionReferenceValue : IEvaluable
{
    public string Name { get; init; } = "";

    public override string ToString() => $"{Name}";
}
