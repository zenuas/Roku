using System;

namespace Command
{
    public class LongOptionAttribute : Attribute
    {
        public String Command { get; }

        public LongOptionAttribute(string cmd) => Command = cmd;
    }
}
