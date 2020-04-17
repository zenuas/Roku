using System;

namespace Command
{
    public class LongOptionAttribute : Attribute
    {
        public String Command { get; private set; }

        public LongOptionAttribute(string cmd)
        {
            Command = cmd;
        }
    }
}
