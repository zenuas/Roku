using Roku.Declare;
using Roku.Manager;

namespace Roku.IntermediateCode;

public class TypeReferenceBind : IOperand
{
    public Operator Operator { get; init; } = Operator.TypeBind;
    public required IEvaluable Name { get; init; }
    public required IStructBody Struct { get; init; }

    public override string ToString() => $"var {Name}: {Struct.Name}";
}
