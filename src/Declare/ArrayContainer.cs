using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Declare
{
    public class ArrayContainer : IEvaluable
    {
        public List<IEvaluable> Values { get; }

        public ArrayContainer(List<IEvaluable> values)
        {
            Values = values;
        }

        public override string ToString() => $"[{Values.Select(x => x.ToString()!).Join(", ")}]";
    }
}
