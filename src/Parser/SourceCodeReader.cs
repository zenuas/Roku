using Extensions;
using System.IO;

namespace Roku.Parser
{
    public class SourceCodeReader : StreamReader
    {
        public int LineNumber { get; protected set; } = 1;
        public int LineColumn { get; protected set; } = 1;

        public SourceCodeReader(Stream stream) : base(stream)
        {
        }

        public override int Read()
        {
            var c = base.Read();
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

        public override string? ReadLine()
        {
            LineNumber += 1;
            LineColumn = 1;
            return base.ReadLine();
        }

        public virtual char ReadChar()
        {
            if (EndOfStream) return '\0';

            return Read().Cast<char>();
        }

        public virtual char PeekChar()
        {
            if (EndOfStream) return '\0';

            return Peek().Cast<char>();
        }
    }
}
