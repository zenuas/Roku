using System;
using System.Reflection;

namespace Roku.Manager;

public class ExternFunction : IFunctionName
{
    public string Name { get; }
    public MethodInfo Function { get; }
    public Type? DeclaringType { get; init; }
    public Assembly Assembly { get; }

    public ExternFunction(string name, MethodInfo f, Assembly asm)
    {
        Name = name;
        Function = f;
        Assembly = asm;
    }

    public override string ToString() => Name;
}
