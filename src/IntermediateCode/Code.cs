using Roku.Declare;

namespace Roku.IntermediateCode
{
    public class Code : IOperand, IReturnBind
    {
        public Operator Operator { get; set; } = Operator.Nop;
        public IEvaluable? Return { get; set; }
        public IEvaluable? Left { get; set; }
        public IEvaluable? Right { get; set; }

        public override string ToString() => (Return is null ? "" : $"{Return} = ") + (Right is null ? $"{Operator} {Left}" : $"{Left} {Operator} {Right}");
    }
}
