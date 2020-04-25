using Extensions;
using NUnit.Framework;
using Roku.Node;

namespace Roku.Tests
{
    public class ParserTest
    {
        [Test]
        public void BlankTest()
        {
            var p = Parse("");
            Assert.AreEqual(p.Statements.Count, 0);
        }

        [Test]
        public void LetTest()
        {
            var p = Parse("var a = 123_456");
            Assert.AreEqual(p.Statements.Count, 1);
            Assert.IsTrue(p.Statements[0] is LetNode);

            var let = p.Statements[0].Cast<LetNode>();
            Assert.AreEqual(let.Var.Name, "a");

            Assert.IsTrue(let.Expression is NumericNode);
            var num = let.Expression.Cast<NumericNode>();
            Assert.AreEqual(num.Format, "123_456");
            Assert.AreEqual(num.Value, 123456);
        }

        public static ProgramNode Parse(string s) => new Parser.Parser().Parse(LexerTest.Read(s)).Cast<ProgramNode>();
    }
}
