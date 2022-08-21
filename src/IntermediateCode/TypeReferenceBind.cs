using Roku.Declare;
using Roku.Manager;

namespace Roku.IntermediateCode;

public class TypeReferenceBind : IOperand
{
    public Operator Operator { get; init; } = Operator.TypeBind;
    public IEvaluable Name { get; }
    public IStructBody Struct { get; }

    public TypeReferenceBind(IEvaluable name, IStructBody sb)
    {
        Name = name;
        Struct = sb;
    }

    public override string ToString() => $"var {Name}: {Struct.Name}";
}
