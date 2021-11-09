using Roku.Declare;

namespace Roku.IntermediateCode;

public class IfCode : IOperand
{
    public Operator Operator { get; init; } = Operator.If;
    public IEvaluable Condition { get; }
    public LabelCode Else { get; }

    public IfCode(IEvaluable cond, LabelCode else_)
    {
        Condition = cond;
        Else = else_;
    }

    public override string ToString() => $"if {Condition} else goto {Else}";
}
