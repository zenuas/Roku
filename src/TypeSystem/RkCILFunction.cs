using System.Collections.Generic;
using System.Reflection;

namespace Roku.TypeSystem
{
    public class RkCILFunction : IFunction
    {
        public string Name { get; }
        public MethodInfo MethodInfo { get; }
        public IType? Return { get; set; } = null;
        public List<IType> Arguments { get; } = new List<IType>();

        public RkCILFunction(string name, MethodInfo method)
        {
            Name = name;
            MethodInfo = method;
        }
    }
}
