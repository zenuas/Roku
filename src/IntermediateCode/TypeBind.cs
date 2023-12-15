using Roku.Declare;

namespace Roku.IntermediateCode;

public class TypeBind : IOperand
{
    public Operator Operator { get; init; } = Operator.TypeBind;
    public required IEvaluable Name { get; init; }
    public required ITypeDefinition Type { get; init; }

    public override string ToString() => $"var {Name}: {Type}";
}
