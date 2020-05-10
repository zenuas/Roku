namespace Roku.IntermediateCode
{
    public class Cast : IOperand, IReturnBind
    {
        public Operator Operator { get; set; } = Operator.Cast;
        public ITypedValue? Return { get; set; }
        public ITypedValue Value { get; }
        public TypeValue Type { get; set; }

        public Cast(ITypedValue value, TypeValue type)
        {
            Value = value;
            Type = type;
        }

        public override string ToString() => $"{(Return is null ? "" : Return.ToString() + " = ")}cast({Value}, {Type})";
    }
}
