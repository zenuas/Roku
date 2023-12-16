﻿using Roku.Declare;
using System.Collections.Generic;

namespace Roku.Manager;

public class TypeMapper : Dictionary<IEvaluable, VariableDetail>
{
    public VariableValue CastBoxCondition = new() { Name = "$cast_box" };
}
