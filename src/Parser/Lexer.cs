

using Extensions;
using Roku.Node;


using System.Collections.Generic;

namespace Roku.Parser;

public partial class Lexer : ILexer<INode>
{
    public static Dictionary<char, Symbols> CreateReservedCharTable()
    {
        var table = new Dictionary<char, Symbols>();
        table['('] = Symbols.__LeftParenthesis;
        table[')'] = Symbols.__RightParenthesis;
        table[','] = Symbols.__Comma;
        table['.'] = Symbols.__FullStop;
        table[':'] = Symbols.__Colon;
        table['?'] = Symbols.__QuestionMark;
        table['['] = Symbols.__LeftSquareBracket;
        table[']'] = Symbols.__RightSquareBracket;
        table['{'] = Symbols.__LeftCurlyBracket;
        table['}'] = Symbols.__RightCurlyBracket;
        return table;
    }

    public static Dictionary<string, Symbols> CreateReservedStringTable()
    {
        var table = new Dictionary<string, Symbols>();
        table["AND2"] = Symbols.AND2;
        table["ARROW"] = Symbols.ARROW;
        table["BEGIN"] = Symbols.BEGIN;
        table["CLASS"] = Symbols.CLASS;
        table["ELSE"] = Symbols.ELSE;
        table["END"] = Symbols.END;
        table["EOL"] = Symbols.EOL;
        table["EQ"] = Symbols.EQ;
        table["FALSE"] = Symbols.FALSE;
        table["FLOAT"] = Symbols.FLOAT;
        table["GT"] = Symbols.GT;
        table["IF"] = Symbols.IF;
        table["IGNORE"] = Symbols.IGNORE;
        table["INSTANCE"] = Symbols.INSTANCE;
        table["IS"] = Symbols.IS;
        table["LAMBDA_START"] = Symbols.LAMBDA_START;
        table["LET"] = Symbols.LET;
        table["LT"] = Symbols.LT;
        table["NULL"] = Symbols.NULL;
        table["NUM"] = Symbols.NUM;
        table["OPE"] = Symbols.OPE;
        table["OR"] = Symbols.OR;
        table["OR2"] = Symbols.OR2;
        table["STR"] = Symbols.STR;
        table["STRUCT"] = Symbols.STRUCT;
        table["SUB"] = Symbols.SUB;
        table["SWITCH"] = Symbols.SWITCH;
        table["THEN"] = Symbols.THEN;
        table["TRUE"] = Symbols.TRUE;
        table["TYPE_PARAM"] = Symbols.TYPE_PARAM;
        table["UNARY"] = Symbols.UNARY;
        table["VAR"] = Symbols.VAR;
        return table;
    }

    public static Token CreateEndOfToken() => new Token { Type = Symbols._END };
}
