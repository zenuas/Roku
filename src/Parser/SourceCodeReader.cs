﻿using System.IO;

namespace Roku.Parser
{
    public class SourceCodeReader
    {
        public TextReader BaseReader { get; }
        public int LineNumber { get; protected set; } = 1;
        public int LineColumn { get; protected set; } = 1;

        public SourceCodeReader(TextReader reader)
        {
            BaseReader = reader;
        }

        public int Read()
        {
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

        public string? ReadLine()
        {
            LineNumber += 1;
            LineColumn = 1;
            return BaseReader.ReadLine();
        }

        public char ReadChar() => EndOfStream ? '\0' : (char)Read();

        public char PeekChar() => EndOfStream ? '\0' : (char)BaseReader.Peek();

        public bool EndOfStream { get => BaseReader.Peek() < 0; }
    }
}
