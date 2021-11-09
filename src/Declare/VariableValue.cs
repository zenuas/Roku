namespace Roku.Declare;

public class VariableValue : IEvaluable
{
    public string Name { get; }

    public VariableValue(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;
}
