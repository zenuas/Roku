using System;

namespace Command;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
public class CommandHelpAttribute : Attribute
{
    public required string Message { get; init; }
}
