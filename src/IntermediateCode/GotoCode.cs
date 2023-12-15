namespace Roku.IntermediateCode;

public class GotoCode : IOperand
{
    public Operator Operator { get; init; } = Operator.Goto;
    public required LabelCode Label { get; init; }

    public override string ToString() => $"goto {Label}";
}
