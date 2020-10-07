using Roku.Declare;

namespace Roku.IntermediateCode
{
    public class Code : IOperand, IReturnBind
    {
        public Operator Operator { get; set; } = Operator.Nop;
        public ITypedValue? Return { get; set; }
        public ITypedValue? Left { get; set; }
        public ITypedValue? Right { get; set; }

        public override string ToString() => (Return is null ? "" : $"{Return} = ") + (Right is null ? $"{Operator} {Left}" : $"{Left} {Operator} {Right}");
    }
}
