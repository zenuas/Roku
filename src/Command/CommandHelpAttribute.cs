using System;

namespace Command;

public class CommandHelpAttribute : Attribute
{
    public string Message { get; }

    public CommandHelpAttribute(string msg) => Message = msg;
}
