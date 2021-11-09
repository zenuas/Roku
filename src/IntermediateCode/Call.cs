using Roku.Declare;

namespace Roku.IntermediateCode;

public class Call : IOperand, IReturnBind
{
    public Operator Operator { get; init; } = Operator.Call;
    public IEvaluable? Return { get; set; }
    public FunctionCallValue Function { get; }

    public Call(FunctionCallValue f)
    {
        Function = f;
    }

    public override string ToString() => $"{(Return is null ? "" : Return.ToString() + " = ")}{Function}";
}
