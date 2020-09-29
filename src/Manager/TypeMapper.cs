using Roku.Declare;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class TypeMapper : Dictionary<ITypedValue, VariableDetail>
    {
        public VariableValue CastBoxCondition = new VariableValue("$cast_box");
    }
}
