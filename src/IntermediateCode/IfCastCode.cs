using Roku.Declare;

namespace Roku.IntermediateCode;

public class IfCastCode : IOperand
{
    public Operator Operator { get; init; } = Operator.IfCast;
    public IEvaluable Name { get; }
    public ITypeDefinition Type { get; }
    public IEvaluable Condition { get; }
    public LabelCode Else { get; }

    public IfCastCode(IEvaluable name, ITypeDefinition type, IEvaluable cond, LabelCode else_)
    {
        Name = name;
        Type = type;
        Condition = cond;
        Else = else_;
    }

    public override string ToString() => $"if {Name}: {Type} = {Condition} else goto {Else}";
}
