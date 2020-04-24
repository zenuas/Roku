using System.Reflection;

namespace Roku.Manager
{
    public class ExternFunction : IFunctionBody
    {
        public string Name { get; }
        public MethodInfo Function { get; }

        public ExternFunction(string name, MethodInfo f)
        {
            Name = name;
            Function = f;
        }
    }
}
