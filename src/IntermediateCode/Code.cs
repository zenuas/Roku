using Roku.Declare;
using System;

namespace Roku.IntermediateCode;

public class Code : IOperand, IReturnBind
{
    public Operator Operator { get; init; } = Operator.Nop;
    public IEvaluable? Return { get; set; }
    public IEvaluable? Left { get => Left_; init => Left_ = value; }

    private IEvaluable? Left_;

    [Obsolete]
    public void LeftReplace(IEvaluable? v) => Left_ = v;

    public override string ToString() => (Return is null ? "" : $"{Return} = ") + $"{Operator} {Left}";
}
