using System;

namespace Roku.Parser;

public class SyntaxErrorException(string message) : Exception(message)
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
}
