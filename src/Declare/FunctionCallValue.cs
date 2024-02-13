using Mina.Extension;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Declare;

public class FunctionCallValue : IEvaluable
{
    public required IEvaluable Function { get; set; }
    public IEvaluable? FirstLookup { get; set; }
    public bool ReceiverToArgumentsInserted { get; set; } = false;
    public List<IEvaluable> Arguments { get; init; } = [];

    public override string ToString() => $"{(FirstLookup is null ? "" : FirstLookup.ToString() + ".")}{Function}({Arguments.Select(x => x.ToString()!).Join(", ")})";
}
