﻿using Mina.Extension;
using Roku.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roku.Parser;

public partial class Lexer : ILexer<INode>
{
    public required SourceCodeReader BaseReader { get; init; }
    public required Parser Parser { get; init; }
    public List<IToken<INode>> Store { get; init; } = [];
    public Stack<int> Indents { get; init; } = new();
    public static Dictionary<string, Symbols> ReservedString2 { get; } = new()
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

    public IToken<INode> PeekToken()
    {
        if (Store.IsEmpty())
        {
        READ_LINE_:
            var line = BaseReader.LineNumber;
            var ts = ReadLineTokens(BaseReader);
            if (ts.First().Symbol == Symbols.EOL) goto READ_LINE_;
            Store.AddRange(ts);

            if (Store[0].Symbol != Symbols._END && (Indents.Count == 0 || Store[0].Value.Indent > Indents.Peek()))
            {
                Store.Insert(0, new TokenNode { Symbol = Symbols.BEGIN, LineNumber = line, Indent = Store[0].Value.Indent });
                Indents.Push(Store[0].Value.Indent);
            }
            else
            {
                var count = 0;
                var head = Store[0];
                while (Indents.Count > 0 && (head.Symbol == Symbols._END || head.Value.Indent < Indents.Peek()))
                {
                    Store.Insert(count, new TokenNode { Symbol = Symbols.END, LineNumber = line, Indent = Indents.Pop() });
                    count++;
                }
            }
        }

    READ_FIRST_:
        var first = Store.First();
        if (IsLambdaStart())
        {
            Store.Insert(0, first = new TokenNode() { Symbol = Symbols.LAMBDA_START });
        }
        else if (first.Symbol == Symbols.EOL && !Parser.IsAccept(first.Symbol))
        {
            Store.Clear();
            Store.AddRange(ReadLineTokens(BaseReader));
            goto READ_FIRST_;
        }
        else if (first.Symbol == Symbols.OPE &&
            first is TokenNode t &&
            t.Name.All(x => x == '>') &&
            Parser.IsAccept(Symbols.GT))
        {
            if (t.Name.Length > 1)
            {
                t.Name = t.Name[1..];
            }
            else
            {
                Store.RemoveAt(0);
            }
            Store.Insert(0, first = new TokenNode() { Symbol = Symbols.GT });
        }
        return first;
    }

    public bool IsLambdaStart()
    {
        if (Store.First().Symbol == Symbols.__LeftParenthesis && Parser.IsAccept(Symbols.LAMBDA_START))
        {
            /*
                lambda : . LAMBDA_START '(' lambda_args ')' typex ARROW lambda_func
            */
            var parentheses = new Stack<Symbols>();
            for (var i = 0; i < Store.Count; i++)
            {
                var current = Store[i].Symbol;
                if (current == Symbols._END || (parentheses.Count == 0 && (current == Symbols.EOL || current == Symbols.__Comma)))
                {
                    return false;
                }
                else if (parentheses.Count > 0 && current == Symbols.EOL)
                {
                    Store.RemoveAt(i);
                    Store.AddRange(ReadLineTokens(BaseReader));
                    i--;
                }
                else if (current == Symbols.__LeftParenthesis ||
                    current == Symbols.__LeftSquareBracket ||
                    current == Symbols.__LeftCurlyBracket)
                {
                    parentheses.Push(current);
                }
                else if (current == Symbols.__RightParenthesis ||
                    current == Symbols.__RightSquareBracket ||
                    current == Symbols.__RightCurlyBracket)
                {
                    if (parentheses.Count == 0) return false;

                    var last = parentheses.Peek();
                    if ((last == Symbols.__LeftParenthesis && current == Symbols.__RightParenthesis) ||
                        (last == Symbols.__LeftSquareBracket && current == Symbols.__RightSquareBracket) ||
                        (last == Symbols.__LeftCurlyBracket && current == Symbols.__RightCurlyBracket))
                    {
                        _ = parentheses.Pop();
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (parentheses.Count == 0 && current == Symbols.ARROW)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public IToken<INode> ReadToken()
    {
        var t = PeekToken();
        Store.RemoveAt(0);
        return t;
    }

    public static List<IToken<INode>> ReadLineTokens(SourceCodeReader reader)
    {
        var (indent, eof) = ReadSkipWhiteSpace(reader, true);
        if (eof is { }) return [eof];

        var comment = ReadSkipComment(reader, indent, true);
        return comment is { } ? [comment] : ReadTokens(reader, indent);
    }

    public static List<IToken<INode>> ReadTokens(SourceCodeReader reader, int indent)
    {
        var ts = new List<IToken<INode>>();
        while (true)
        {
            var line = reader.LineNumber;
            var col = reader.LineColumn;
            var t = ReadToken(reader, ts.LastOrDefault().To(x => x is TokenNode t && t.Name == "."));
            t.Value.LineNumber = line;
            t.Value.LineColumn = col;
            t.Value.Indent = indent;
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

    public static (int Indent, IToken<INode>? Token) ReadSkipWhiteSpace(SourceCodeReader reader, bool only_first_eof_makes_eof)
    {
        var line = reader.LineNumber;
        var col = reader.LineColumn;
        if (reader.EndOfStream) return (0, only_first_eof_makes_eof ? new TokenNode { Symbol = Symbols._END, LineNumber = line, LineColumn = col } : new TokenNode { Symbol = Symbols.EOL, LineNumber = line, LineColumn = col });

        var c = reader.PeekChar();
        var indent = 0;
        while (char.IsWhiteSpace(c))
        {
            indent++;
            _ = reader.ReadChar();

            if (c == '\r' && reader.PeekChar() == '\n') _ = reader.Read();
            if (c == '\r' || c == '\n') return (indent, new TokenNode { Symbol = Symbols.EOL, LineNumber = line, LineColumn = col });

            if (reader.EndOfStream) return (indent, new TokenNode { Symbol = Symbols.EOL, LineNumber = line, LineColumn = col });

            line = reader.LineNumber;
            col = reader.LineColumn;
            c = reader.PeekChar();
        }
        return (indent, null);
    }

    public static IToken<INode>? ReadSkipComment(SourceCodeReader reader, int indent, bool enable_block_comment)
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
                    if (reader.EndOfStream) return new TokenNode { Symbol = Symbols._END, LineNumber = line, LineColumn = col };
                    var s = reader.ReadLine()!;
                    if (CountIndent(s) == indent)
                    {
                        var without_indent = s.Skip(indent).ToStringByChars();
                        if (without_indent.StartsWith("###") && without_indent.FindFirstIndex(x => x != '#') == block_count) break;
                    }
                }
            }
            return new TokenNode { Symbol = Symbols.EOL, LineNumber = line, LineColumn = col };
        }
        return null;
    }

    public static IToken<INode> ReadToken(SourceCodeReader reader, bool isprev_dot)
    {
        var c = reader.PeekChar();
        if (ReservedChar.TryGetValue(c, out var value)) return new TokenNode { Symbol = value, Name = reader.ReadChar().ToString() };

        switch (c)
        {
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
                            return new TokenNode { Symbol = Symbols.OPE, Name = "===" };
                        }
                        return new TokenNode { Symbol = Symbols.OPE, Name = "==" };
                    }
                    else if (c2 == '>')
                    {
                        _ = reader.ReadChar();
                        return new TokenNode { Symbol = Symbols.ARROW, Name = "=>" };
                    }
                    return new TokenNode { Symbol = Symbols.EQ };
                }

            case '_':
                {
                    _ = reader.ReadChar();
                    var c2 = reader.PeekChar();
                    if (c2 != '_' && !IsAlphabet(c2)) return new TokenNode { Symbol = Symbols.IGNORE };

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
                            return ReadNumberOrFloat(reader);
                    }
                }

            default:
                if (isprev_dot && IsNumber(c)) return ReadDecimal(reader);
                if (IsNumber(c)) return ReadNumberOrFloat(reader);
                if (IsAlphabet(c)) return ReadVariable(reader);
                if (IsOperator(c)) return ReadOperator(reader);
                break;
        }

        throw new SyntaxErrorException("syntax error") { LineNumber = reader.LineNumber, LineColumn = reader.LineColumn };
    }

    public static TokenNode ReadVariable(SourceCodeReader reader) => ReadVariable(reader, new StringBuilder());

    public static TokenNode ReadVariable(SourceCodeReader reader, StringBuilder s)
    {
        while (IsWord(reader.PeekChar())) _ = s.Append(reader.ReadChar());
        var name = s.ToString();
        return ReservedString2.TryGetValue(name, out var value) ? new TokenNode { Symbol = value, Name = name }
            : new TokenNode { Symbol = Symbols.VAR, Name = name };
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

    public static NumericNode ReadNumber(SourceCodeReader reader, int base_, string prefix, Func<char, bool> isnum)
    {
        var (value, format) = ReadNumberText(reader, prefix, isnum);
        return new NumericNode { Value = Convert.ToUInt32(value, base_), Format = format };
    }

    public static IToken<INode> ReadNumberOrFloat(SourceCodeReader reader, string prefix = "")
    {
        var (value, format) = ReadNumberText(reader, prefix, IsFloatingNumber);
        return value.FindFirstIndex(c => c == '.') >= 0
            ? new FloatingNumericNode { Value = Convert.ToDouble(value), Format = format }
            : new NumericNode { Value = Convert.ToUInt32(value, 10), Format = format };
    }

    public static NumericNode ReadDecimal(SourceCodeReader reader) => ReadNumber(reader, 10, "", IsNumber);

    public static NumericNode ReadHexadecimal(SourceCodeReader reader, string prefix = "0x") => ReadNumber(reader, 16, prefix, IsHexadecimal);

    public static NumericNode ReadOctal(SourceCodeReader reader, string prefix = "0o") => ReadNumber(reader, 8, prefix, IsOctal);

    public static NumericNode ReadBinary(SourceCodeReader reader, string prefix = "0b") => ReadNumber(reader, 2, prefix, IsBinary);

    public static StringNode ReadString(SourceCodeReader reader)
    {
        var start = reader.ReadChar();
        var s = new StringBuilder();
        while (!reader.EndOfStream)
        {
            var c = reader.ReadChar();
            if (c == start) break;
            _ = s.Append(c);
        }
        return new StringNode { Value = s.ToString() };
    }

    public static TokenNode ReadOperator(SourceCodeReader reader)
    {
        var s = new StringBuilder();
        while (!reader.EndOfStream)
        {
            if (!IsOperator(reader.PeekChar())) break;
            _ = s.Append(reader.ReadChar());
        }
        if (reader.PeekChar() == '=') _ = s.Append(reader.ReadChar());
        var ope = s.ToString();
        return new TokenNode
        {
            Symbol = ope switch
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

    public static bool IsOctal(char c) => c >= '0' && c <= '7';

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
