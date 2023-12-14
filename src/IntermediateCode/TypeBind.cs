using Roku.Declare;

namespace Roku.IntermediateCode;

public class TypeBind(IEvaluable name, ITypeDefinition type) : IOperand
{
    public Operator Operator { get; init; } = Operator.TypeBind;
    public IEvaluable Name { get; } = name;
    public ITypeDefinition Type { get; } = type;

    public override string ToString() => $"var {Name}: {Type}";
}
