using Extensions;
using Roku.Node;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roku.Parser
{
    public partial class Lexer : ILexer<INode>
    {
        public SourceCodeReader BaseReader { get; }
        public List<Token> Store { get; } = new List<Token>();

        public Lexer(SourceCodeReader reader)
        {
            BaseReader = reader;
        }

        public IToken<INode> PeekToken()
        {
            if (Store.IsNull()) Store.AddRange(ReadLineTokens(BaseReader));
            return Store.First();
        }

        public IToken<INode> ReadToken()
        {
            var t = PeekToken();
            Store.RemoveAt(0);
            return t;
        }

        public static List<Token> ReadLineTokens(SourceCodeReader reader)
        {
            var (indent, eof) = ReadSkipWhiteSpace(reader, true);
            if (eof is { }) return new List<Token> { eof };

            var comment = ReadSkipComment(reader, indent, true);
            if (comment is { }) return new List<Token> { comment };

            var ts = new List<Token>();
            while (true)
            {
                var line = reader.LineNumber;
                var col = reader.LineColumn;
                var t = ReadToken(reader);
                t.LineNumber = line;
                t.LineColumn = col;
                t.Indent = indent;
                ts.Add(t);
                var (_, eol) = ReadSkipWhiteSpace(reader, false);
                if (eol is { })
                {
                    ts.Add(eol);
                    break;
                }
                var linecomment = ReadSkipComment(reader, indent, false);
                if (linecomment is { })
                {
                    ts.Add(linecomment);
                    break;
                }
            }
            return ts;
        }

        public static int CountIndent(string s)
        {
            if (s.Length == 0 || !char.IsWhiteSpace(s[0])) return 0;
            var indent = s.FindFirstIndex(x => !char.IsWhiteSpace(x));
            return indent >= 0 ? indent : s.Length;
        }

        public static (int Indent, Token? Token) ReadSkipWhiteSpace(SourceCodeReader reader, bool only_first_eof_makes_eof)
        {
            var line = reader.LineNumber;
            var col = reader.LineColumn;
            if (reader.EndOfStream) return (0, new Token { Type = only_first_eof_makes_eof ? Symbols.EOF : Symbols.EOL, LineNumber = line, LineColumn = col });

            var c = reader.PeekChar();
            var indent = 0;
            while (char.IsWhiteSpace(c))
            {
                indent++;
                _ = reader.ReadChar();

                if (c == '\r' && reader.PeekChar() == '\n') _ = reader.Read();
                if (c == '\r' || c == '\n') return (indent, new Token { Type = Symbols.EOL, LineNumber = line, LineColumn = col });

                if (reader.EndOfStream) return (indent, new Token { Type = Symbols.EOL, LineNumber = line, LineColumn = col });

                line = reader.LineNumber;
                col = reader.LineColumn;
                c = reader.PeekChar();
            }
            return (indent, null);
        }

        public static Token? ReadSkipComment(SourceCodeReader reader, int indent, bool enable_block_comment)
        {
            var line = reader.LineNumber;
            var col = reader.LineColumn;
            var c = reader.PeekChar();
            if (c == '#')
            {
                var comment = reader.ReadLine()!;
                if (enable_block_comment && comment.StartsWith("###"))
                {
                    var block_count = comment.FindFirstIndex(x => x != '#');
                    while (true)
                    {
                        if (reader.EndOfStream) return new Token { Type = Symbols.EOF, LineNumber = line, LineColumn = col };
                        var s = reader.ReadLine()!;
                        if (CountIndent(s) == indent)
                        {
                            var without_indent = s.Drop(indent).ToStringByChars();
                            if (without_indent.StartsWith("###") && without_indent.FindFirstIndex(x => x != '#') == block_count) break;
                        }
                    }
                }
                return new Token { Type = Symbols.EOL, LineNumber = line, LineColumn = col };
            }
            return null;
        }

        public static Token ReadToken(SourceCodeReader reader)
        {
            var c = reader.PeekChar();

            switch (c)
            {
                case '.':
                    break;

                case '"':
                    return ReadString(reader);

                case '=':
                    _ = reader.ReadChar();
                    return new Token { Type = Symbols.EQ };

                case '_':
                    _ = reader.ReadChar();
                    var c2 = reader.PeekChar();
                    if (c2 != '_' && !IsAlphabet(c2)) return new Token { Type = Symbols.IGNORE };

                    var s = new StringBuilder(c);
                    while (c2 == '_')
                    {
                        s.Append(c2);
                        c2 = reader.PeekChar();
                    }
                    if (IsAlphabet(c2)) return ReadVariable(reader, s);
                    break;

                default:
                    if (IsNumber(c)) return ReadDecimal(reader);
                    if (IsAlphabet(c)) return ReadVariable(reader);
                    break;
            }

            throw new Exception();
        }

        public static Token ReadVariable(SourceCodeReader reader) => ReadVariable(reader, new StringBuilder());

        public static Token ReadVariable(SourceCodeReader reader, StringBuilder s)
        {
            while (IsWord(reader.PeekChar())) s.Append(reader.ReadChar());
            return new Token { Type = Symbols.VAR, Name = s.ToString() };
        }

        public static Token ReadDecimal(SourceCodeReader reader)
        {
            var s = new StringBuilder();
            var n = 0;
            while (!reader.EndOfStream)
            {
                var c = reader.PeekChar();
                if (c == '_') { /* read skip */ }
                else if (IsNumber(c))
                {
                    n = (n * 10) + (c - '0');
                }
                else
                {
                    break;
                }
                _ = reader.ReadChar();
                s.Append(c);
            }
            return new Token { Type = Symbols.NUM, Name = s.ToString(), Value = null };
        }

        public static Token ReadString(SourceCodeReader reader)
        {
            var start = reader.ReadChar();
            var s = new StringBuilder();
            while (!reader.EndOfStream)
            {
                var c = reader.ReadChar();
                if (c == start) break;
                s.Append(c);
            }
            return new Token { Type = Symbols.STR, Name = s.ToString() };
        }

        public static bool IsNumber(char c) => c >= '0' && c <= '9';

        public static bool IsNoneZeroNumber(char c) => c >= '1' && c <= '9';

        public static bool IsBinary(char c) => c == '0' || c == '1';

        public static bool IsOctal(char c) => c >= '1' && c <= '7';

        public static bool IsHexadecimal(char c) => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

        public static bool IsLowerAlphabet(char c) => c >= 'a' && c <= 'z';

        public static bool IsUpperAlphabet(char c) => c >= 'A' && c <= 'Z';

        public static bool IsAlphabet(char c) => IsLowerAlphabet(c) || IsUpperAlphabet(c);

        public static bool IsWord(char c) => c == '_' || IsLowerAlphabet(c) || IsUpperAlphabet(c) || IsNumber(c);

        public static bool IsOperator(char c) =>
            c == '-' ||
            c == '+' ||
            c == '*' ||
            c == '/' ||
            c == '<' ||
            c == '>' ||
            c == '!' ||
            c == '%' ||
            c == '^' ||
            c == '&' ||
            c == '\\' ||
            c == '|' ||
            c == '?' ||
            c == '~';
    }
}
