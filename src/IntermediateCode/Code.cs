using Roku.Declare;
using System;

namespace Roku.IntermediateCode;

public class Code : IOperand, IReturnBind
{
    public Operator Operator { get; init; } = Operator.Nop;
    public IEvaluable? Return { get; set; }
    public IEvaluable? Left { get => Left_; init => Left_ = value; }
    public IEvaluable? Right { get => Right_; init => Right_ = value; }

    private IEvaluable? Left_;
    private IEvaluable? Right_;

    [Obsolete]
    public void LeftReplace(IEvaluable? v) => Left_ = v;

    [Obsolete]
    public void RightReplace(IEvaluable? v) => Left_ = v;

    public override string ToString() => (Return is null ? "" : $"{Return} = ") + (Right is null ? $"{Operator} {Left}" : $"{Left} {Operator} {Right}");
}
