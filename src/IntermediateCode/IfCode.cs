using Roku.Declare;
using System;

namespace Roku.IntermediateCode;

public class IfCode(IEvaluable cond, LabelCode else_) : IOperand
{
    public Operator Operator { get; } = Operator.If;
    public IEvaluable Condition { get; private set; } = cond;
    public LabelCode Else { get; } = else_;

    [Obsolete]
    public void ConditionReplace(IEvaluable v) => Condition = v;

    public override string ToString() => $"if {Condition} else goto {Else}";
}
