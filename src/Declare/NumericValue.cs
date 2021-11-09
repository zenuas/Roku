namespace Roku.Declare;

public class NumericValue : IEvaluable
{
    public uint Value { get; init; }

    public override string ToString() => Value.ToString();
}
