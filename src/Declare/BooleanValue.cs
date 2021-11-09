namespace Roku.Declare;

public class BooleanValue : IEvaluable
{
    public bool Value { get; }

    public BooleanValue(bool b)
    {
        Value = b;
    }

    public override string ToString() => Value.ToString();
}
