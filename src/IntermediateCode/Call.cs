using Roku.Manager;

namespace Roku.IntermediateCode
{
    public class Call : IOperand, IReturnBind
    {
        public Operator Operator { get; set; } = Operator.Call;
        public ITypedValue? Return { get; set; }
        public FunctionCallValue Function { get; }
        public FunctionCaller? Caller { get; set; }

        public Call(FunctionCallValue f)
        {
            Function = f;
        }

        public override string ToString() => $"{(Return is null ? "" : Return.ToString() + " = ")}{Function}";
    }
}
