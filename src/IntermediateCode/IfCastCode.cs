using Roku.Declare;

namespace Roku.IntermediateCode;

public class IfCastCode : IOperand
{
    public Operator Operator { get; } = Operator.IfCast;
    public required IEvaluable Name { get; init; }
    public required ITypeDefinition Type { get; init; }
    public required IEvaluable Condition { get => Condition_; init => Condition_ = value; }
    public required LabelCode Else { get; set; }

    public IEvaluable Condition_ = default!;

    public void ConditionReplace(IEvaluable v) => Condition_ = v;

    public override string ToString() => $"if {Name}: {Type} = {Condition} else goto {Else}";
}
