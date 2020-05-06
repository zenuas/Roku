using System;
using System.Reflection;

namespace Roku.Manager
{
    public class ExternFunction : IFunctionBody
    {
        public string Name { get; }
        public MethodInfo Function { get; }
        public Type? DeclaringType { get; set; }
        public Assembly? Assembly { get; set; }

        public ExternFunction(string name, MethodInfo f)
        {
            Name = name;
            Function = f;
        }

        public override string ToString() => Name;
    }
}
