namespace Roku.Declare;

public class StringValue : IEvaluable
{
    public string Value { get; init; } = "";

    public override string ToString() => $"\"{Value}\"";
}
