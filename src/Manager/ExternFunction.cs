using System;
using System.Reflection;

namespace Roku.Manager;

public class ExternFunction : IFunctionName
{
    public required string Name { get; init; }
    public required MethodInfo Function { get; init; }
    public Type? DeclaringType { get; init; }
    public required Assembly Assembly { get; init; }

    public override string ToString() => Name;
}
