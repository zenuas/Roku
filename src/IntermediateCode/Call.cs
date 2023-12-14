using Roku.Declare;

namespace Roku.IntermediateCode;

public class Call(FunctionCallValue f) : IOperand, IReturnBind
{
    public Operator Operator { get; } = Operator.Call;
    public IEvaluable? Return { get; set; }
    public FunctionCallValue Function { get; } = f;

    public override string ToString() => $"{(Return is null ? "" : Return.ToString() + " = ")}{Function}";
}
