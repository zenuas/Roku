using System;

namespace Command;

public class ShortOptionAttribute : Attribute
{
    public char Command { get; }

    public ShortOptionAttribute(char cmd) => Command = cmd;
}
