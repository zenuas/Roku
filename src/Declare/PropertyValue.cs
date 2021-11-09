namespace Roku.Declare;

public class PropertyValue : IEvaluable
{
    public IEvaluable Left { get; }
    public string Right { get; }

    public PropertyValue(IEvaluable left, string right)
    {
        Left = left;
        Right = right;
    }

    public override string ToString() => $"{Left}.{Right}";
}
