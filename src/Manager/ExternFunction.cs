using System;
using System.Reflection;

namespace Roku.Manager;

public class ExternFunction(string name, MethodInfo f, Assembly asm) : IFunctionName
{
    public string Name { get; } = name;
    public MethodInfo Function { get; } = f;
    public Type? DeclaringType { get; init; }
    public Assembly Assembly { get; } = asm;

    public override string ToString() => Name;
}
