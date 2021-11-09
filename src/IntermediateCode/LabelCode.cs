namespace Roku.IntermediateCode;

public class LabelCode : IOperand
{
    public Operator Operator { get; set; } = Operator.Label;
    public string Name { get; set; } = "Label";

    public override string ToString() => $"_{Name}:";
}
