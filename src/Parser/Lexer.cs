

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
            table[','] = Symbols.__x2C;
            table['.'] = Symbols.__x2E;
            table['['] = Symbols.__x5B;
            table['{'] = Symbols.__x7B;
            return table;
        }

        public static Dictionary<string, Symbols> CreateReservedStringTable()
        {
            var table = new Dictionary<string, Symbols>();
            table["BEGIN"] = Symbols.BEGIN;
            table["END"] = Symbols.END;
            table["EOF"] = Symbols.EOF;
            table["EOL"] = Symbols.EOL;
            table["EQ"] = Symbols.EQ;
            table["FALSE"] = Symbols.FALSE;
            table["IF"] = Symbols.IF;
            table["IGNORE"] = Symbols.IGNORE;
            table["LET"] = Symbols.LET;
            table["line"] = Symbols.line;
            table["NULL"] = Symbols.NULL;
            table["NUM"] = Symbols.NUM;
            table["STR"] = Symbols.STR;
            table["SUB"] = Symbols.SUB;
            table["TRUE"] = Symbols.TRUE;
            table["UNARY"] = Symbols.UNARY;
            table["VAR"] = Symbols.VAR;
            return table;
        }
    }
}
