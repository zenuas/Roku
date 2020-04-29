using System.Reflection.Emit;

namespace Roku.IntermediateCode
{
    public class IfCode : IOperand
    {
        public Operator Operator { get; set; } = Operator.If;
        public ITypedValue Condition { get; set; }
        public LabelCode Else { get; set; }

        public IfCode(ITypedValue cond, LabelCode else_)
        {
            Condition = cond;
            Else = else_;
        }

        public override string ToString() => $"if {Condition} goto {Else}";
    }
}
