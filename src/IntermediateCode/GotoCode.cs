namespace Roku.IntermediateCode;

public class GotoCode : IOperand
{
    public Operator Operator { get; } = Operator.Goto;
    public required LabelCode Label { get; init; }

    public override string ToString() => $"goto {Label}";
}
