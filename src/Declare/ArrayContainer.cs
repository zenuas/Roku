using Mina.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Declare;

public class ArrayContainer : IEvaluable
{
    public required List<IEvaluable> Values { get; init; }

    public override string ToString() => $"[{Values.Select(x => x.ToString()!).Join(", ")}]";
}
