namespace Roku.IntermediateCode;

public class LabelCode : IOperand
{
    public Operator Operator { get; init; } = Operator.Label;
    public string Name { get; init; } = "Label";

    public override string ToString() => $"_{Name}:";
}
