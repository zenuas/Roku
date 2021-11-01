using Extensions;
using Roku.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roku.Parser
{
    public partial class Lexer : ILexer<INode>
    {
        public SourceCodeReader BaseReader { get; }
        public Parser? Parser { get; set; }
        public List<Token> Store { get; } = new List<Token>();
        public Stack<int> Indents { get; } = new Stack<int>();
        public static Dictionary<char, Symbols> ReservedChar { get; } = CreateReservedCharTable();
        public static Dictionary<string, Symbols> ReservedString { get; } = new Dictionary<string, Symbols>
            {
                { "var", Symbols.LET },
                { "struct", Symbols.STRUCT },
                { "class", Symbols.CLASS },
                { "instance", Symbols.INSTANCE },
                { "sub", Symbols.SUB },
                { "if", Symbols.IF },
                { "then", Symbols.THEN },
                { "else", Symbols.ELSE },
                { "switch", Symbols.SWITCH },
                { "true", Symbols.TRUE },
                { "false", Symbols.FALSE },
                { "null", Symbols.NULL },
                { "is", Symbols.IS },
            };

        public Lexer(SourceCodeReader reader)
        {
            BaseReader = reader;
        }

        public IToken<INode> PeekToken()
        {
            if (Store.IsEmpty())
            {
            READ_LINE_:
                var line = BaseReader.LineNumber;
                var ts = ReadLineTokens(BaseReader, Parser?.TokenStack.LastOrDefault());
                if (ts.First().Type == Symbols.EOL) goto READ_LINE_;
                Store.AddRange(ts);

                if (!Store[0].EndOfToken && (Indents.Count == 0 || Store[0].Indent > Indents.Peek()))
                {
                    Store.Insert(0, new Token { Type = Symbols.BEGIN, LineNumber = line, Indent = Store[0].Indent });
                    Indents.Push(Store[0].Indent);
                }
                else
                {
                    var count = 0;
                    var head = Store[0];
                    while (Indents.Count > 0 && (head.EndOfToken || head.Indent < Indents.Peek()))
                    {
                        Store.Insert(count, new Token { Type = Symbols.END, LineNumber = line, Indent = Indents.Pop() });
                        count++;
                    }
                }
            }

        READ_FIRST_:
            var first = Store.First();
            if (Parser is { })
            {
                if (first.Type == Symbols.EOL && !Parser.IsAccept(first))
                {
                    Store.Clear();
                    Store.AddRange(ReadLineTokens(BaseReader, Parser?.TokenStack.LastOrDefault()));
                    goto READ_FIRST_;
                }
                else if (first.Type == Symbols.OPE &&
                    first.Name.All(x => x == '>') &&
                    Parser.IsAccept(new Token() { Type = Symbols.GT }))
                {
                    if (first.Name.Length > 1)
                    {
                        first.Name = first.Name[1..];
                    }
                    else
                    {
                        Store.RemoveAt(0);
                    }
                    Store.Insert(0, first = new Token() { Type = Symbols.GT });
                }
            }
            return first;
        }

        public IToken<INode> ReadToken()
        {
            var t = PeekToken();
            Store.RemoveAt(0);
            return t;
        }

        public static List<Token> ReadLineTokens(SourceCodeReader reader, Token? last_token)
        {
            var (indent, eof) = ReadSkipWhiteSpace(reader, true);
            if (eof is { }) return new List<Token> { eof };

            var comment = ReadSkipComment(reader, indent, true);
            if (comment is { }) return new List<Token> { comment };

            return ReadTokens(reader, indent, last_token);
        }

        public static List<Token> ReadTokens(SourceCodeReader reader, int indent, Token? last_token)
        {
            var ts = new List<Token>();
            while (true)
            {
                var line = reader.LineNumber;
                var col = reader.LineColumn;
                var t = ReadToken(reader, ts.Count > 0 ? ts.Last() : last_token);
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
            if (reader.EndOfStream) return (0, only_first_eof_makes_eof ? CreateEndOfToken() : new Token { Type = Symbols.EOL, LineNumber = line, LineColumn = col });

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
                        if (reader.EndOfStream) return CreateEndOfToken();
                        var s = reader.ReadLine()!;
                        if (CountIndent(s) == indent)
                        {
                            var without_indent = s.Skip(indent).ToStringByChars();
                            if (without_indent.StartsWith("###") && without_indent.FindFirstIndex(x => x != '#') == block_count) break;
                        }
                    }
                }
                return new Token { Type = Symbols.EOL, LineNumber = line, LineColumn = col };
            }
            return null;
        }

        public static Token ReadToken(SourceCodeReader reader, Token? last_token)
        {
            var c = reader.PeekChar();
            if (ReservedChar.ContainsKey(c)) return new Token { Type = ReservedChar[c], Name = reader.ReadChar().ToString() };

            switch (c)
            {
                case '.':
                    break;

                case '"':
                    return ReadString(reader);

                case '=':
                    {
                        _ = reader.ReadChar();
                        var c2 = reader.PeekChar();
                        if (c2 == '=')
                        {
                            _ = reader.ReadChar();
                            if (reader.PeekChar() == '=')
                            {
                                _ = reader.ReadChar();
                                return new Token { Type = Symbols.OPE, Name = "===" };
                            }
                            return new Token { Type = Symbols.OPE, Name = "==" };
                        }
                        else if (c2 == '>')
                        {
                            _ = reader.ReadChar();
                            return new Token { Type = Symbols.ARROW, Name = "=>" };
                        }
                        return new Token { Type = Symbols.EQ };
                    }

                case '_':
                    {
                        _ = reader.ReadChar();
                        var c2 = reader.PeekChar();
                        if (c2 != '_' && !IsAlphabet(c2)) return new Token { Type = Symbols.IGNORE };

                        var s = new StringBuilder(c);
                        while (c2 == '_')
                        {
                            _ = s.Append(c2);
                            c2 = reader.PeekChar();
                        }
                        if (IsAlphabet(c2)) return ReadVariable(reader, s);
                    }
                    break;

                case '0':
                    {
                        _ = reader.ReadChar();
                        var base_ = reader.ReadChar();
                        switch (base_)
                        {
                            case 'x': return ReadHexadecimal(reader, "0x");
                            case 'o': return ReadOctal(reader, "0o");
                            case 'b': return ReadBinary(reader, "0b");
                            default:
                                reader.UnRead(base_);
                                reader.UnRead('0');
                                return ReadDecimal(reader, "", !(last_token is { } t && t.Type == Symbols.__x2E));
                        }
                    }

                default:
                    if (IsNumber(c)) return ReadDecimal(reader, "", !(last_token is { } t && t.Type == Symbols.__x2E));
                    if (IsAlphabet(c)) return ReadVariable(reader);
                    if (IsOperator(c)) return ReadOperator(reader);
                    break;
            }

            throw new SyntaxErrorException("syntax error") { LineNumber = reader.LineNumber, LineColumn = reader.LineColumn };
        }

        public static Token ReadVariable(SourceCodeReader reader) => ReadVariable(reader, new StringBuilder());

        public static Token ReadVariable(SourceCodeReader reader, StringBuilder s)
        {
            while (IsWord(reader.PeekChar())) s.Append(reader.ReadChar());
            var name = s.ToString();
            return ReservedString.ContainsKey(name)
                ? new Token { Type = ReservedString[name], Name = name }
                : new Token { Type = Symbols.VAR, Name = name };
        }

        public static (string Value, string Format) ReadNumberText(SourceCodeReader reader, string prefix, Func<char, bool> isnum)
        {
            var s = new StringBuilder(prefix);
            var n = new StringBuilder();
            while (!reader.EndOfStream)
            {
                var c = reader.PeekChar();
                if (c == '_') { /* read skip */ }
                else if (isnum(c))
                {
                    _ = n.Append(c);
                }
                else
                {
                    break;
                }
                _ = s.Append(reader.ReadChar());
            }
            return (n.ToString(), s.ToString());
        }

        public static Token ReadNumber(SourceCodeReader reader, int base_, string prefix, Func<char, bool> isnum)
        {
            var (value, format) = ReadNumberText(reader, prefix, isnum);
            return new Token { Type = Symbols.NUM, Name = format, Value = new NumericNode { Value = Convert.ToUInt32(value, base_), Format = format } };
        }

        public static Token ReadNumberOrFloat(SourceCodeReader reader, string prefix, bool floating_enable)
        {
            var (value, format) = ReadNumberText(reader, prefix, floating_enable ? (Func<char, bool>)IsFloatingNumber : IsNumber);
            if (floating_enable && value.FindFirstIndex(c => c == '.') >= 0)
            {
                return new Token { Type = Symbols.FLOAT, Name = format, Value = new FloatingNumericNode { Value = Convert.ToDouble(value), Format = format } };
            }
            else
            {
                return new Token { Type = Symbols.NUM, Name = format, Value = new NumericNode { Value = Convert.ToUInt32(value, 10), Format = format } };
            }
        }

        public static Token ReadDecimal(SourceCodeReader reader, string prefix = "", bool floating_enable = true) => ReadNumberOrFloat(reader, prefix, floating_enable);

        public static Token ReadHexadecimal(SourceCodeReader reader, string prefix = "0x") => ReadNumber(reader, 16, prefix, IsHexadecimal);

        public static Token ReadOctal(SourceCodeReader reader, string prefix = "0o") => ReadNumber(reader, 8, prefix, IsOctal);

        public static Token ReadBinary(SourceCodeReader reader, string prefix = "0b") => ReadNumber(reader, 2, prefix, IsBinary);

        public static Token ReadString(SourceCodeReader reader)
        {
            var start = reader.ReadChar();
            var s = new StringBuilder();
            while (!reader.EndOfStream)
            {
                var c = reader.ReadChar();
                if (c == start) break;
                _ = s.Append(c);
            }
            return new Token { Type = Symbols.STR, Name = s.ToString() };
        }

        public static Token ReadOperator(SourceCodeReader reader)
        {
            var s = new StringBuilder();
            while (!reader.EndOfStream)
            {
                if (!IsOperator(reader.PeekChar())) break;
                s.Append(reader.ReadChar());
            }
            if (reader.PeekChar() == '=') s.Append(reader.ReadChar());
            var ope = s.ToString();
            return new Token
            {
                Type = ope switch
                {
                    "<" => Symbols.LT,
                    ">" => Symbols.GT,
                    "|" => Symbols.OR,
                    "&&" => Symbols.AND2,
                    "||" => Symbols.OR2,
                    _ => Symbols.OPE,
                },
                Name = ope
            };
        }

        public static bool IsNumber(char c) => c >= '0' && c <= '9';

        public static bool IsNoneZeroNumber(char c) => c >= '1' && c <= '9';

        public static bool IsBinary(char c) => c == '0' || c == '1';

        public static bool IsOctal(char c) => c >= '1' && c <= '7';

        public static bool IsHexadecimal(char c) => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

        public static bool IsFloatingNumber(char c) => c == '.' || IsNumber(c);

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
