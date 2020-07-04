namespace Roku.IntermediateCode
{
    public class IfCastCode : IOperand
    {
        public Operator Operator { get; set; } = Operator.IfCast;
        public ITypedValue Name { get; set; }
        public ITypeDefinition Type { get; set; }
        public ITypedValue Condition { get; set; }
        public LabelCode Else { get; set; }

        public IfCastCode(ITypedValue name, ITypeDefinition type, ITypedValue cond, LabelCode else_)
        {
            Name = name;
            Type = type;
            Condition = cond;
            Else = else_;
        }

        public override string ToString() => $"if {Name}: {Type} = {Condition} else goto {Else}";
    }
}
