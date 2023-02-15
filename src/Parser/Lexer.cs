using Extensions;
using Roku.Node;

using System.Collections.Generic;

namespace Roku.Parser;

public partial class Lexer : ILexer<INode>
{
    public static Dictionary<char, Symbols> ReservedChar { get; } = new()
        {
            { ',', Symbols.__Comma },
            { ':', Symbols.__Colon },
            { '?', Symbols.__QuestionMark },
            { '.', Symbols.__FullStop },
            { '(', Symbols.__LeftParenthesis },
            { ')', Symbols.__RightParenthesis },
            { '[', Symbols.__LeftSquareBracket },
            { ']', Symbols.__RightSquareBracket },
            { '{', Symbols.__LeftCurlyBracket },
            { '}', Symbols.__RightCurlyBracket },
        };
    public static Dictionary<string, Symbols> ReservedString { get; } = new()
        {
            { "AND2", Symbols.AND2 },
            { "ARROW", Symbols.ARROW },
            { "BEGIN", Symbols.BEGIN },
            { "CLASS", Symbols.CLASS },
            { "ELSE", Symbols.ELSE },
            { "END", Symbols.END },
            { "EOL", Symbols.EOL },
            { "EQ", Symbols.EQ },
            { "FALSE", Symbols.FALSE },
            { "FLOAT", Symbols.FLOAT },
            { "GT", Symbols.GT },
            { "IF", Symbols.IF },
            { "IGNORE", Symbols.IGNORE },
            { "INSTANCE", Symbols.INSTANCE },
            { "IS", Symbols.IS },
            { "LAMBDA_START", Symbols.LAMBDA_START },
            { "LET", Symbols.LET },
            { "LT", Symbols.LT },
            { "NULL", Symbols.NULL },
            { "NUM", Symbols.NUM },
            { "OPE", Symbols.OPE },
            { "OR", Symbols.OR },
            { "OR2", Symbols.OR2 },
            { "STR", Symbols.STR },
            { "STRUCT", Symbols.STRUCT },
            { "SUB", Symbols.SUB },
            { "SWITCH", Symbols.SWITCH },
            { "THEN", Symbols.THEN },
            { "TRUE", Symbols.TRUE },
            { "TYPE_PARAM", Symbols.TYPE_PARAM },
            { "UNARY", Symbols.UNARY },
            { "VAR", Symbols.VAR },
        };
}
