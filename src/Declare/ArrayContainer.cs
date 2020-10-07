using Extensions;
using System.Collections.Generic;

namespace Roku.Declare
{
    public class ArrayContainer : IEvaluable
    {
        public List<IEvaluable> Values { get; }

        public ArrayContainer(List<IEvaluable> values)
        {
            Values = values;
        }

        public override string ToString() => $"[{Values.Map(x => x.ToString()!).Join(", ")}]";
    }
}
