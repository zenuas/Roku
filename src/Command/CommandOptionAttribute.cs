using System;

namespace Mina.Command;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
public class CommandOptionAttribute : Attribute
{
    public string Command { get; init; }
    public bool IsShortOption { get; init; } = false;

    public CommandOptionAttribute() => Command = "";

    public CommandOptionAttribute(string cmd)
    {
        Command = cmd;
        IsShortOption = false;
    }

    public CommandOptionAttribute(char cmd)
    {
        Command = cmd.ToString();
        IsShortOption = true;
    }
}
