using Roku.Declare;
using System;

namespace Roku.IntermediateCode;

public class BindCode : IOperand, IReturnBind
{
    public Operator Operator { get; } = Operator.Bind;
    public IEvaluable? Return { get; set; }
    public IEvaluable? Value { get => Value_; init => Value_ = value; }

    private IEvaluable? Value_;

    [Obsolete]
    public void LeftReplace(IEvaluable? v) => Value_ = v;

    public override string ToString() => (Return is null ? "" : $"{Return} = ") + $"{Operator} {Value}";
}
