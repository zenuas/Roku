using Extensions;
using NUnit.Framework;
using Roku.Node;
using Roku.Parser;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Roku.Tests;

public class LexerTest
{
    [Test]
    public void CountIndentTest()
    {
        Assert.AreEqual(Lexer.CountIndent("  a"), 2);
        Assert.AreEqual(Lexer.CountIndent("  \ta"), 3);
        Assert.AreEqual(Lexer.CountIndent("a"), 0);
    }

    [Test]
    public void BlankSourceTest()
    {
        var ts1 = Tokens("");
        Assert.AreEqual(ts1.Count, 1);
        Assert.AreEqual(ts1[0].Symbol, Symbols._END);

        var ts2 = Tokens(" ");
        Assert.AreEqual(ts2.Count, 1);
        Assert.AreEqual(ts2[0].Symbol, Symbols._END);

        var ts3 = Tokens(" \n ");
        Assert.AreEqual(ts3.Count, 1);
        Assert.AreEqual(ts3[0].Symbol, Symbols._END);
    }

    [Test]
    public void OneLineTest()
    {
        var ts1 = Tokens("a = 123_456");
        Assert.AreEqual(ts1.Count, 5);
        Assert.IsTrue(ts1.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.VAR,
                Symbols.EQ,
                Symbols.NUM,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.AreEqual(ts1[1].Cast<TokenNode>().Name, "a");
        Assert.AreEqual(ts1[3].Cast<NumericNode>().Format, "123_456");

        var ts2 = Tokens("b = \"abc123\"");
        Assert.AreEqual(ts2.Count, 5);
        Assert.IsTrue(ts2.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.VAR,
                Symbols.EQ,
                Symbols.STR,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.AreEqual(ts2[1].Cast<TokenNode>().Name, "b");
        Assert.AreEqual(ts2[3].Cast<StringNode>().Value, "abc123");
    }

    [Test]
    public void NumberTest()
    {
        var ts1 = Tokens("0o123_456");
        Assert.AreEqual(ts1.Count, 3);
        Assert.IsTrue(ts1.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.NUM,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.AreEqual(ts1[1].Cast<NumericNode>().Format, "0o123_456");
        Assert.AreEqual(ts1[1].Cast<NumericNode>().Value, 42_798u);

        var ts2 = Tokens("0x123_ABC");
        Assert.AreEqual(ts2.Count, 3);
        Assert.IsTrue(ts2.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.NUM,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.AreEqual(ts2[1].Cast<NumericNode>().Format, "0x123_ABC");
        Assert.AreEqual(ts2[1].Cast<NumericNode>().Value, 1_194_684u);

        var ts3 = Tokens("0b101_010");
        Assert.AreEqual(ts3.Count, 3);
        Assert.IsTrue(ts3.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.NUM,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.AreEqual(ts3[1].Cast<NumericNode>().Format, "0b101_010");
        Assert.AreEqual(ts3[1].Cast<NumericNode>().Value, 42u);

        var ts4 = Tokens("0p1");
        Assert.AreEqual(ts4.Count, 4);
        Assert.IsTrue(ts4.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.NUM,
                Symbols.VAR,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.AreEqual(ts4[1].Cast<NumericNode>().Format, "0");
        Assert.AreEqual(ts4[1].Cast<NumericNode>().Value, 0u);
        Assert.AreEqual(ts4[2].Cast<TokenNode>().Name, "p1");

        var ts5 = Tokens("12_3.4_56");
        Assert.AreEqual(ts5.Count, 3);
        Assert.IsTrue(ts5.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.FLOAT,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.AreEqual(ts5[1].Cast<FloatingNumericNode>().Format, "12_3.4_56");
        Assert.AreEqual(ts5[1].Cast<FloatingNumericNode>().Value, 123.456);

        var ts6 = Tokens("x.1.2");
        Assert.AreEqual(ts6.Count, 5);
        Assert.IsTrue(ts6.Zip(new Symbols[]
            {
                Symbols.BEGIN,
                Symbols.VAR,
                Symbols.__FullStop,
                Symbols.FLOAT,
                Symbols._END,
            }).All(x => x.First.Symbol == x.Second));
        Assert.AreEqual(ts6[1].Cast<TokenNode>().Name, "x");
        Assert.AreEqual(ts6[3].Cast<FloatingNumericNode>().Format, "1.2");
    }

    public static Lexer Read(string s) => new Lexer() { BaseReader = new SourceCodeReader(new StringReader(s)), Parser = new Parser.Parser() };

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
