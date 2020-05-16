namespace Roku.IntermediateCode
{
    public class TypeBind : IOperand
    {
        public Operator Operator { get; set; } = Operator.TypeBind;
        public ITypedValue Name { get; set; }
        public TypeValue Type { get; set; }

        public TypeBind(ITypedValue name, TypeValue type)
        {
            Name = name;
            Type = type;
        }

        public override string ToString() => $"var {Name}: {Type}";
    }
}
