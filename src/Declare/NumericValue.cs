namespace Roku.Declare;

public class NumericValue : IEvaluable
{
    public uint Value { get; }

    public NumericValue(uint n)
    {
        Value = n;
    }

    public override string ToString() => Value.ToString();
}
