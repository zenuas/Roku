using Roku.Declare;

namespace Roku.IntermediateCode
{
    public class IfCastCode : IOperand
    {
        public Operator Operator { get; set; } = Operator.IfCast;
        public IEvaluable Name { get; set; }
        public ITypeDefinition Type { get; set; }
        public IEvaluable Condition { get; set; }
        public LabelCode Else { get; set; }

        public IfCastCode(IEvaluable name, ITypeDefinition type, IEvaluable cond, LabelCode else_)
        {
            Name = name;
            Type = type;
            Condition = cond;
            Else = else_;
        }

        public override string ToString() => $"if {Name}: {Type} = {Condition} else goto {Else}";
    }
}
