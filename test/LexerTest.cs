using Extensions;
using NUnit.Framework;
using Roku.Node;
using Roku.Parser;
using System.Collections.Generic;
using System.IO;

namespace Roku.Tests
{
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
            Assert.AreEqual(ts1[0].Type, Symbols._END);

            var ts2 = Tokens(" ");
            Assert.AreEqual(ts2.Count, 1);
            Assert.AreEqual(ts2[0].Type, Symbols._END);

            var ts3 = Tokens(" \n ");
            Assert.AreEqual(ts3.Count, 1);
            Assert.AreEqual(ts3[0].Type, Symbols._END);
        }

        [Test]
        public void OneLineTest()
        {
            var ts1 = Tokens("a = 123_456");
            Assert.AreEqual(ts1.Count, 7);
            Assert.IsTrue(ts1.Zip(new Symbols[]
                {
                    Symbols.BEGIN,
                    Symbols.VAR,
                    Symbols.EQ,
                    Symbols.NUM,
                    Symbols.EOL,
                    Symbols.END,
                    Symbols._END,
                }).And(x => x.First.Type == x.Second));
            Assert.AreEqual(ts1[1].Name, "a");
            Assert.AreEqual(ts1[3].Name, "123_456");

            var ts2 = Tokens("b = \"abc123\"");
            Assert.AreEqual(ts2.Count, 7);
            Assert.IsTrue(ts2.Zip(new Symbols[]
                {
                    Symbols.BEGIN,
                    Symbols.VAR,
                    Symbols.EQ,
                    Symbols.STR,
                    Symbols.EOL,
                    Symbols.END,
                    Symbols._END,
                }).And(x => x.First.Type == x.Second));
            Assert.AreEqual(ts2[1].Name, "b");
            Assert.AreEqual(ts2[3].Name, "abc123");
        }

        [Test]
        public void IndentErrorTest()
        {
            var ts = Tokens(
@"    a = 123_456
  b = ""abc123""");
            Assert.AreEqual(ts.Count, 11);
            Assert.IsTrue(ts.Zip(new Symbols[]
                {
                    Symbols.BEGIN,
                    Symbols.VAR,
                    Symbols.EQ,
                    Symbols.NUM,
                    Symbols.EOL,
                    Symbols.END,
                    Symbols.VAR,
                    Symbols.EQ,
                    Symbols.STR,
                    Symbols.EOL,
                    Symbols._END,
                }).And(x => x.First.Type == x.Second));
            Assert.AreEqual(ts[1].Name, "a");
            Assert.AreEqual(ts[3].Name, "123_456");
            Assert.AreEqual(ts[6].Name, "b");
            Assert.AreEqual(ts[8].Name, "abc123");
        }

        [Test]
        public void NumberTest()
        {
            var ts1 = Tokens("0o123_456");
            Assert.AreEqual(ts1.Count, 5);
            Assert.IsTrue(ts1.Zip(new Symbols[]
                {
                    Symbols.BEGIN,
                    Symbols.NUM,
                    Symbols.EOL,
                    Symbols.END,
                    Symbols._END,
                }).And(x => x.First.Type == x.Second));
            Assert.AreEqual(ts1[1].Name, "0o123_456");
            Assert.AreEqual(ts1[1].Value!.Cast<NumericNode>().Value, 42_798u);

            var ts2 = Tokens("0x123_ABC");
            Assert.AreEqual(ts2.Count, 5);
            Assert.IsTrue(ts2.Zip(new Symbols[]
                {
                    Symbols.BEGIN,
                    Symbols.NUM,
                    Symbols.EOL,
                    Symbols.END,
                    Symbols._END,
                }).And(x => x.First.Type == x.Second));
            Assert.AreEqual(ts2[1].Name, "0x123_ABC");
            Assert.AreEqual(ts2[1].Value!.Cast<NumericNode>().Value, 1_194_684u);

            var ts3 = Tokens("0b101_010");
            Assert.AreEqual(ts3.Count, 5);
            Assert.IsTrue(ts3.Zip(new Symbols[]
                {
                    Symbols.BEGIN,
                    Symbols.NUM,
                    Symbols.EOL,
                    Symbols.END,
                    Symbols._END,
                }).And(x => x.First.Type == x.Second));
            Assert.AreEqual(ts3[1].Name, "0b101_010");
            Assert.AreEqual(ts3[1].Value!.Cast<NumericNode>().Value, 42u);

            var ts4 = Tokens("0p1");
            Assert.AreEqual(ts4.Count, 6);
            Assert.IsTrue(ts4.Zip(new Symbols[]
                {
                    Symbols.BEGIN,
                    Symbols.NUM,
                    Symbols.VAR,
                    Symbols.EOL,
                    Symbols.END,
                    Symbols._END,
                }).And(x => x.First.Type == x.Second));
            Assert.AreEqual(ts4[1].Name, "0");
            Assert.AreEqual(ts4[1].Value!.Cast<NumericNode>().Value, 0u);
            Assert.AreEqual(ts4[2].Name, "p1");

            var ts5 = Tokens("12_3.4_56");
            Assert.AreEqual(ts5.Count, 5);
            Assert.IsTrue(ts5.Zip(new Symbols[]
                {
                    Symbols.BEGIN,
                    Symbols.FLOAT,
                    Symbols.EOL,
                    Symbols.END,
                    Symbols._END,
                }).And(x => x.First.Type == x.Second));
            Assert.AreEqual(ts5[1].Name, "12_3.4_56");
            Assert.AreEqual(ts5[1].Value!.Cast<FloatingNumericNode>().Value, 123.456);

            var ts6 = Tokens("x.1.2");
            Assert.AreEqual(ts6.Count, 9);
            Assert.IsTrue(ts6.Zip(new Symbols[]
                {
                    Symbols.BEGIN,
                    Symbols.VAR,
                    Symbols.__x2E,
                    Symbols.NUM,
                    Symbols.__x2E,
                    Symbols.NUM,
                    Symbols.EOL,
                    Symbols.END,
                    Symbols._END,
                }).And(x => x.First.Type == x.Second));
            Assert.AreEqual(ts6[1].Name, "x");
            Assert.AreEqual(ts6[3].Name, "1");
            Assert.AreEqual(ts6[5].Name, "2");
        }

        public static Lexer Read(string s) => new Lexer(new SourceCodeReader(new StringReader(s)));

        public static List<Token> Tokens(string s)
        {
            var lex = Read(s);
            var xs = new List<Token>();
            while (true)
            {
                var t = lex.ReadToken().Cast<Token>();
                xs.Add(t);
                if (t.Type == Symbols._END) break;
            }
            return xs;
        }

    }
}
