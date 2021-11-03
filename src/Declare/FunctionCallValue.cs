﻿using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Declare
{
    public class FunctionCallValue : IEvaluable
    {
        public IEvaluable Function { get; set; }
        public IEvaluable? FirstLookup { get; set; }
        public bool ReceiverToArgumentsInserted { get; set; } = false;
        public List<IEvaluable> Arguments { get; } = new List<IEvaluable>();

        public FunctionCallValue(IEvaluable f)
        {
            Function = f;
        }

        public override string ToString() => $"{(FirstLookup is null ? "" : FirstLookup.ToString() + ".")}{Function}({Arguments.Select(x => x.ToString()!).Join(", ")})";
    }
}
