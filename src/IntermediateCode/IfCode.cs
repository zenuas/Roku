using Roku.Declare;
using System;

namespace Roku.IntermediateCode;

public class IfCode : IOperand
{
    public Operator Operator { get; init; } = Operator.If;
    public IEvaluable Condition { get; private set; }
    public LabelCode Else { get; }

    public IfCode(IEvaluable cond, LabelCode else_)
    {
        Condition = cond;
        Else = else_;
    }

    [Obsolete]
    public void ConditionReplace(IEvaluable v) => Condition = v;

    public override string ToString() => $"if {Condition} else goto {Else}";
}
