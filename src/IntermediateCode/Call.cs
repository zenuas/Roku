﻿using Roku.Declare;

namespace Roku.IntermediateCode;

public class Call : IOperand, IReturnBind
{
    public Operator Operator { get; init; } = Operator.Call;
    public IEvaluable? Return { get; set; }
    public required FunctionCallValue Function { get; init; }

    public override string ToString() => $"{(Return is null ? "" : $"{Return} = ")}{Function}";
}
