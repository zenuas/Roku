namespace Roku.Declare;

public class VariableValue : IEvaluable
{
    public string Name { get; init; } = "";

    public override string ToString() => Name;
}
