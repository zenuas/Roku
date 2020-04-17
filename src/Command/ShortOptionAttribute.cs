using System;

namespace Command
{
    public class ShortOptionAttribute : Attribute
    {
        public char Command { get; private set; }

        public ShortOptionAttribute(char cmd)
        {
            Command = cmd;
        }
    }
}
