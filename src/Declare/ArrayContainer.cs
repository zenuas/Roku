using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Declare;

public class ArrayContainer(List<IEvaluable> values) : IEvaluable
{
    public List<IEvaluable> Values { get; } = values;

    public override string ToString() => $"[{Values.Select(x => x.ToString()!).Join(", ")}]";
}
