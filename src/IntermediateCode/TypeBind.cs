using Roku.Declare;

namespace Roku.IntermediateCode
{
    public class TypeBind : IOperand
    {
        public Operator Operator { get; set; } = Operator.TypeBind;
        public IEvaluable Name { get; set; }
        public ITypeDefinition Type { get; set; }

        public TypeBind(IEvaluable name, ITypeDefinition type)
        {
            Name = name;
            Type = type;
        }

        public override string ToString() => $"var {Name}: {Type}";
    }
}
