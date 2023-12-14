using Roku.Declare;
using Roku.Manager;

namespace Roku.IntermediateCode;

public class TypeReferenceBind(IEvaluable name, IStructBody sb) : IOperand
{
    public Operator Operator { get; init; } = Operator.TypeBind;
    public IEvaluable Name { get; } = name;
    public IStructBody Struct { get; } = sb;

    public override string ToString() => $"var {Name}: {Struct.Name}";
}
