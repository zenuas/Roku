namespace Roku.Declare;

public class BooleanValue : IEvaluable
{
    public bool Value { get; init; }

    public override string ToString() => Value.ToString();
}
