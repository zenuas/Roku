using NUnit.Framework;
using Roku.Parser;

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

    }
}
