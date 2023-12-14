using Roku.Declare;
using System;

namespace Roku.IntermediateCode;

public class IfCastCode(IEvaluable name, ITypeDefinition type, IEvaluable cond, LabelCode else_) : IOperand
{
    public Operator Operator { get; } = Operator.IfCast;
    public IEvaluable Name { get; } = name;
    public ITypeDefinition Type { get; } = type;
    public IEvaluable Condition { get; private set; } = cond;
    public LabelCode Else { get; } = else_;

    [Obsolete]
    public void ConditionReplace(IEvaluable v) => Condition = v;

    public override string ToString() => $"if {Name}: {Type} = {Condition} else goto {Else}";
}
