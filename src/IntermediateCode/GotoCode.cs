namespace Roku.IntermediateCode;

public class GotoCode(LabelCode label) : IOperand
{
    public Operator Operator { get; } = Operator.Goto;
    public LabelCode Label { get; set; } = label;

    public override string ToString() => $"goto {Label}";
}
