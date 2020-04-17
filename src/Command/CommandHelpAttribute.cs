using System;

namespace Command
{
    public class CommandHelpAttribute : Attribute
    {
        public string Message { get; private set; }

        public CommandHelpAttribute(string msg) => Message = msg;
    }
}
