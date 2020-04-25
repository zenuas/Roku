using Extensions;
using NUnit.Framework;
using Roku.Parser;
using System.Collections.Generic;
using System.IO;

namespace Roku.Tests
{
    class LexerTest
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
            Assert.AreEqual(ts1[0].Type, Symbols.EOF);

            var ts2 = Tokens(" ");
            Assert.AreEqual(ts2.Count, 1);
            Assert.AreEqual(ts2[0].Type, Symbols.EOF);

            var ts3 = Tokens(" \n ");
            Assert.AreEqual(ts3.Count, 1);
            Assert.AreEqual(ts3[0].Type, Symbols.EOF);
        }

        [Test]
        public void OneLineTest()
        {
            var ts1 = Tokens("a = 1");
            Assert.AreEqual(ts1.Count, 7);
            Assert.IsTrue(ts1.Zip(new Symbols[]
                {
                    Symbols.BEGIN,
                    Symbols.VAR,
                    Symbols.EQ,
                    Symbols.NUM,
                    Symbols.EOL,
                    Symbols.END,
                    Symbols.EOF,
                }).And(x => x.First.Type == x.Second));
        }

        public Lexer Read(string s) => new Lexer(new SourceCodeReader(new StringReader(s)));

        public List<Token> Tokens(string s)
        {
            var lex = Read(s);
            var xs = new List<Token>();
            while (true)
            {
                var t = lex.ReadToken().Cast<Token>();
                xs.Add(t);
                if (t.Type == Symbols.EOF) break;
            }
            return xs;
        }

    }
}
