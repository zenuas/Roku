namespace Roku.Declare;

public class ImplicitReturnValue : IEvaluable
{
    public string Name { get; init; } = "_";

    public override string ToString() => Name;
}
