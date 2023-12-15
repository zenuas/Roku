namespace Roku.Declare;

public class VariableValue : IEvaluable
{
    public required string Name { get; init; }

    public override string ToString() => Name;
}
