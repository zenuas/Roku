using Roku.Declare;

namespace Roku.IntermediateCode;

public class Code : IOperand, IReturnBind
{
    public Operator Operator { get; init; } = Operator.Nop;
    public IEvaluable? Return { get; set; }
    public IEvaluable? Left { get; init; }
    public IEvaluable? Right { get; init; }

    public override string ToString() => (Return is null ? "" : $"{Return} = ") + (Right is null ? $"{Operator} {Left}" : $"{Left} {Operator} {Right}");
}
