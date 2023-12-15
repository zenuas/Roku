namespace Roku.Declare;

public class PropertyValue : IEvaluable
{
    public required IEvaluable Left { get; init; }
    public required string Right { get; init; }

    public override string ToString() => $"{Left}.{Right}";
}
