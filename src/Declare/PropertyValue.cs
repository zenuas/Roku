namespace Roku.Declare;

public class PropertyValue(IEvaluable left, string right) : IEvaluable
{
    public IEvaluable Left { get; } = left;
    public string Right { get; } = right;

    public override string ToString() => $"{Left}.{Right}";
}
