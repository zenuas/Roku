using Roku.Declare;

namespace Roku.IntermediateCode;

public class TypeBind : IOperand
{
    public Operator Operator { get; init; } = Operator.TypeBind;
    public IEvaluable Name { get; }
    public ITypeDefinition Type { get; }

    public TypeBind(IEvaluable name, ITypeDefinition type)
    {
        Name = name;
        Type = type;
    }

    public override string ToString() => $"var {Name}: {Type}";
}
