using Mina.Extension;
using Roku.Node;
using Roku.Parser;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Roku.Test;

public class LexerTest
{
    [Fact]
    public void CountIndentTest()
    {
        Assert.Equal(Lexer.CountIndent("  a"), 2);
        Assert.Equal(Lexer.CountIndent("  \ta"), 3);
        Assert.Equal(Lexer.CountIndent("a"), 0);
    }

    [Fact]
    public void BlankSourceTest()
    {
        var ts1 = Tokens("");
        Assert.Equal(ts1.Count, 1);
        Assert.Equal(ts1[0].Symbol, Symbols._END);

        var ts2 = Tokens(" ");
        Assert.Equal(ts2.Count, 1);
        Assert.Equal(ts2[0].Symbol, Symbols._END);

        var ts3 = Tokens(" \n ");
        Assert.Equal(ts3.Count, 1);
        Assert.Equal(ts3[0].Symbol, Symbols._END);
    }

    [Fact]
    public void OneLineTest()
    {
        var ts1 = Tokens("a = 123_456");
        Assert.Equal(ts1.Count, 5);
        Assert.True(ts1.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.VAR,
                Symbols.EQ,
                Symbols.NUM,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.Equal(ts1[1].Cast<TokenNode>().Name, "a");
        Assert.Equal(ts1[3].Cast<NumericNode>().Format, "123_456");

        var ts2 = Tokens("b = \"abc123\"");
        Assert.Equal(ts2.Count, 5);
        Assert.True(ts2.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.VAR,
                Symbols.EQ,
                Symbols.STR,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.Equal(ts2[1].Cast<TokenNode>().Name, "b");
        Assert.Equal(ts2[3].Cast<StringNode>().Value, "abc123");
    }

    [Fact]
    public void NumberTest()
    {
        var ts1 = Tokens("0o123_456");
        Assert.Equal(ts1.Count, 3);
        Assert.True(ts1.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.NUM,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.Equal(ts1[1].Cast<NumericNode>().Format, "0o123_456");
        Assert.Equal(ts1[1].Cast<NumericNode>().Value, 42_798u);

        var ts2 = Tokens("0x123_ABC");
        Assert.Equal(ts2.Count, 3);
        Assert.True(ts2.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.NUM,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.Equal(ts2[1].Cast<NumericNode>().Format, "0x123_ABC");
        Assert.Equal(ts2[1].Cast<NumericNode>().Value, 1_194_684u);

        var ts3 = Tokens("0b101_010");
        Assert.Equal(ts3.Count, 3);
        Assert.True(ts3.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.NUM,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.Equal(ts3[1].Cast<NumericNode>().Format, "0b101_010");
        Assert.Equal(ts3[1].Cast<NumericNode>().Value, 42u);

        var ts4 = Tokens("0p1");
        Assert.Equal(ts4.Count, 4);
        Assert.True(ts4.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.NUM,
                Symbols.VAR,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.Equal(ts4[1].Cast<NumericNode>().Format, "0");
        Assert.Equal(ts4[1].Cast<NumericNode>().Value, 0u);
        Assert.Equal(ts4[2].Cast<TokenNode>().Name, "p1");

        var ts5 = Tokens("12_3.4_56");
        Assert.Equal(ts5.Count, 3);
        Assert.True(ts5.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.FLOAT,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.Equal(ts5[1].Cast<FloatingNumericNode>().Format, "12_3.4_56");
        Assert.Equal(ts5[1].Cast<FloatingNumericNode>().Value, 123.456);

        var ts6 = Tokens("x.1.2");
        Assert.Equal(ts6.Count, 7);
        Assert.True(ts6.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.VAR,
                Symbols.__FullStop,
                Symbols.NUM,
                Symbols.__FullStop,
                Symbols.NUM,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.Equal(ts6[1].Cast<TokenNode>().Name, "x");
        Assert.Equal(ts6[3].Cast<NumericNode>().Format, "1");
        Assert.Equal(ts6[5].Cast<NumericNode>().Format, "2");
    }

    public static Lexer Read(string s) => new() { BaseReader = new SourceCodeReader { BaseReader = new StringReader(s) }, Parser = new Parser.Parser() };

    public static List<IToken<INode>> Tokens(string s)
    {
        var lex = Read(s);
        var xs = new List<IToken<INode>>();
        while (true)
        {
            var t = lex.ReadToken();
            xs.Add(t);
            if (t.Symbol == Symbols._END) break;
        }
        return xs;
    }

}
