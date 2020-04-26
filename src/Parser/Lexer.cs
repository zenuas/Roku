

using Extensions;
using Roku.Node;


using System.Collections.Generic;

namespace Roku.Parser
{
    public partial class Lexer : ILexer<INode>
    {
        public static Dictionary<char, Symbols> CreateReservedCharTable()
        {
            var table = new Dictionary<char, Symbols>();
            table['('] = Symbols.__x28;
            table[')'] = Symbols.__x29;
            table[','] = Symbols.__x2C;
            table['.'] = Symbols.__x2E;
            table[':'] = Symbols.__x3A;
            table['['] = Symbols.__x5B;
            table['{'] = Symbols.__x7B;
            return table;
        }

        public static Dictionary<string, Symbols> CreateReservedStringTable()
        {
            var table = new Dictionary<string, Symbols>();
            table["and"] = Symbols.and;
            table["AND2"] = Symbols.AND2;
            table["BEGIN"] = Symbols.BEGIN;
            table["block"] = Symbols.block;
            table["END"] = Symbols.END;
            table["EOL"] = Symbols.EOL;
            table["EQ"] = Symbols.EQ;
            table["FALSE"] = Symbols.FALSE;
            table["GT"] = Symbols.GT;
            table["IF"] = Symbols.IF;
            table["IGNORE"] = Symbols.IGNORE;
            table["LET"] = Symbols.LET;
            table["LT"] = Symbols.LT;
            table["NULL"] = Symbols.NULL;
            table["NUM"] = Symbols.NUM;
            table["ope"] = Symbols.ope;
            table["OPE"] = Symbols.OPE;
            table["or"] = Symbols.or;
            table["OR"] = Symbols.OR;
            table["OR2"] = Symbols.OR2;
            table["STR"] = Symbols.STR;
            table["SUB"] = Symbols.SUB;
            table["TRUE"] = Symbols.TRUE;
            table["UNARY"] = Symbols.UNARY;
            table["VAR"] = Symbols.VAR;
            return table;
        }

        public static Token CreateEndOfToken() => new Token { Type = Symbols._END };
    }
}
