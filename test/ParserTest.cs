using Extensions;
using NUnit.Framework;
using Roku.Node;

namespace Roku.Tests;

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

    [Test]
    public void ExpressionTest()
    {
        var p = Parse("var a = 1 + 2 - 3");
        Assert.AreEqual(p.Statements.Count, 1);
        Assert.IsTrue(p.Statements[0] is LetNode);

        var let = p.Statements[0].Cast<LetNode>();
        Assert.AreEqual(let.Var.Name, "a");

        Assert.IsTrue(let.Expression is FunctionCallNode);
        var minus = let.Expression.Cast<FunctionCallNode>();
        Assert.IsTrue(minus.Arguments[0] is FunctionCallNode);
        var plus = minus.Arguments[0].Cast<FunctionCallNode>();

        Assert.IsTrue(plus.Expression is TokenNode);
        Assert.IsTrue(minus.Expression is TokenNode);
        Assert.IsTrue(plus.Arguments[0] is NumericNode);
        Assert.IsTrue(plus.Arguments[1] is NumericNode);
        Assert.IsTrue(minus.Arguments[1] is NumericNode);

        Assert.AreEqual(plus.Expression.Cast<TokenNode>().Token.Name, "+");
        Assert.AreEqual(minus.Expression.Cast<TokenNode>().Token.Name, "-");
        Assert.AreEqual(plus.Arguments[0].Cast<NumericNode>().Value, 1);
        Assert.AreEqual(plus.Arguments[1].Cast<NumericNode>().Value, 2);
        Assert.AreEqual(minus.Arguments[1].Cast<NumericNode>().Value, 3);
    }

    [Test]
    public void CallTest()
    {
        var p = Parse("print(\"hello world\")");
        Assert.AreEqual(p.Statements.Count, 1);
        Assert.IsTrue(p.Statements[0] is FunctionCallNode);

        var call = p.Statements[0].Cast<FunctionCallNode>();
        Assert.IsTrue(call.Expression is VariableNode);
        var f = call.Expression.Cast<VariableNode>();
        Assert.AreEqual(f.Name, "print");

        Assert.AreEqual(call.Arguments.Count, 1);
        Assert.IsTrue(call.Arguments[0] is StringNode);
        var str = call.Arguments[0].Cast<StringNode>();
        Assert.AreEqual(str.Value, "hello world");
    }

    [Test]
    public void FunctionTest()
    {
        var p = Parse(@"
sub fn(s: String, n: Int)
    var x = 12
");
        Assert.AreEqual(p.Statements.Count, 0);
        Assert.AreEqual(p.Functions.Count, 1);

        var fn = p.Functions[0];
        Assert.AreEqual(fn.Name.Name, "fn");
        Assert.AreEqual(fn.Arguments.Count, 2);
        Assert.AreEqual(fn.Arguments[0].Name.Name, "s");
        Assert.AreEqual(fn.Arguments[0].Type.Name, "String");
        Assert.AreEqual(fn.Arguments[1].Name.Name, "n");
        Assert.AreEqual(fn.Arguments[1].Type.Name, "Int");

        Assert.AreEqual(fn.Statements.Count, 1);
        Assert.IsTrue(fn.Statements[0] is LetNode);
        var let = fn.Statements[0].Cast<LetNode>();
        Assert.AreEqual(let.Var.Name, "x");
        Assert.IsTrue(let.Expression is NumericNode);

        var num = let.Expression.Cast<NumericNode>();
        Assert.AreEqual(num.Format, "12");
        Assert.AreEqual(num.Value, 12);
    }

    [Test]
    public void ContinueEolTest()
    {
        var p = Parse(@"
sub fn(
        s: String,
        n: Int,
    )
    var x = 12
");
        Assert.AreEqual(p.Statements.Count, 0);
        Assert.AreEqual(p.Functions.Count, 1);

        var fn = p.Functions[0];
        Assert.AreEqual(fn.Name.Name, "fn");
        Assert.AreEqual(fn.Arguments.Count, 2);
        Assert.AreEqual(fn.Arguments[0].Name.Name, "s");
        Assert.AreEqual(fn.Arguments[0].Type.Name, "String");
        Assert.AreEqual(fn.Arguments[1].Name.Name, "n");
        Assert.AreEqual(fn.Arguments[1].Type.Name, "Int");

        Assert.AreEqual(fn.Statements.Count, 1);
        Assert.IsTrue(fn.Statements[0] is LetNode);
        var let = fn.Statements[0].Cast<LetNode>();
        Assert.AreEqual(let.Var.Name, "x");
        Assert.IsTrue(let.Expression is NumericNode);

        var num = let.Expression.Cast<NumericNode>();
        Assert.AreEqual(num.Format, "12");
        Assert.AreEqual(num.Value, 12);
    }

    public static ProgramNode Parse(string s)
    {
        var lex = LexerTest.Read(s);
        return lex.Parser.Parse(lex).Cast<ProgramNode>();
    }
}
