using System;

namespace Mina.Command;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
public class CommandHelpAttribute(string s) : Attribute
{
    public string Message { get; init; } = s;
}
