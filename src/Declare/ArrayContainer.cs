using Extensions;
using System.Collections.Generic;

namespace Roku.Declare
{
    public class ArrayContainer : ITypedValue
    {
        public List<ITypedValue> Values { get; }

        public ArrayContainer(List<ITypedValue> values)
        {
            Values = values;
        }

        public override string ToString() => $"[{Values.Map(x => x.ToString()!).Join(", ")}]";
    }
}
