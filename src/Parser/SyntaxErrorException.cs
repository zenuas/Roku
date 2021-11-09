using System;

namespace Roku.Parser;

public class SyntaxErrorException : Exception
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }

    public SyntaxErrorException(string message) : base(message)
    {
    }
}
