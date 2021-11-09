namespace Roku.Declare;

public class FloatingNumericValue : IEvaluable
{
    public double Value { get; init; }

    public override string ToString() => Value.ToString();
}
