using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Roku.Parser;

public class SourceCodeReader(TextReader reader)
{
    public TextReader BaseReader { get; } = reader;
    public int LineNumber { get; protected set; } = 1;
    public int LineColumn { get; protected set; } = 1;
    public List<char> Buffer { get; } = [];

    public int Read()
    {
        if (Buffer.Count > 0)
        {
            var pop = Buffer.Last();
            Buffer.RemoveAt(Buffer.Count - 1);
            LineColumn += 1;
            return pop;
        }
        var c = BaseReader.Read();
        if (c == '\n')
        {
            LineNumber += 1;
            LineColumn = 1;
        }
        else
        {
            LineColumn += 1;
        }
        return c;
    }

    public void UnRead(char c)
    {
        Buffer.Add(c);
        LineColumn -= 1;
    }

    public string? ReadLine()
    {
        Buffer.Clear();
        LineNumber += 1;
        LineColumn = 1;
        return BaseReader.ReadLine();
    }

    public char ReadChar() => EndOfStream ? '\0' : (char)Read();

    public char PeekChar() => EndOfStream ? '\0' : Buffer.Count > 0 ? Buffer.Last() : (char)BaseReader.Peek();

    public bool EndOfStream => Buffer.Count == 0 && BaseReader.Peek() < 0;
}
