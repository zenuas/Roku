using Roku.Declare;

namespace Roku.IntermediateCode;

public class IfCode : IOperand
{
    public Operator Operator { get; init; } = Operator.If;
    public required IEvaluable Condition { get => Condition_; init => Condition_ = value; }
    public required LabelCode Else { get; init; }

    public IEvaluable Condition_ = default!;

    public void ConditionReplace(IEvaluable v) => Condition_ = v;

    public override string ToString() => $"if {Condition} else goto {Else}";
}
