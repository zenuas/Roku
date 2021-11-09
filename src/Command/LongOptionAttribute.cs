using System;

namespace Command;

public class LongOptionAttribute : Attribute
{
    public string Command { get; }

    public LongOptionAttribute(string cmd) => Command = cmd;
}
