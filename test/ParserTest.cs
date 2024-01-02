using Mina.Extensions;
using Roku.Node;
using Xunit;

namespace Roku.Tests;

public class ParserTest
{
    [Fact]
    public void BlankTest()
    {
        var p = Parse("");
        Assert.Equal(p.Statements.Count, 0);
    }

    [Fact]
    public void LetTest()
    {
        var p = Parse("var a = 123_456");
        Assert.Equal(p.Statements.Count, 1);
        _ = Assert.IsType<LetNode>(p.Statements[0]);

        var let = p.Statements[0].Cast<LetNode>();
        Assert.Equal(let.Var.Name, "a");

        _ = Assert.IsType<NumericNode>(let.Expression);
        var num = let.Expression.Cast<NumericNode>();
        Assert.Equal(num.Format, "123_456");
        Assert.Equal(num.Value, 123456u);
    }

    [Fact]
    public void FloatTest()
    {
        var p = Parse("var a = 123.456");
        Assert.Equal(p.Statements.Count, 1);
        _ = Assert.IsType<LetNode>(p.Statements[0]);

        var let = p.Statements[0].Cast<LetNode>();
        Assert.Equal(let.Var.Name, "a");

        _ = Assert.IsType<FloatingNumericNode>(let.Expression);
        var num = let.Expression.Cast<FloatingNumericNode>();
        Assert.Equal(num.Format, "123.456");
        Assert.Equal(num.Value, 123.456);
    }

    [Fact]
    public void ExpressionTest()
    {
        var p = Parse("var a = 1 + 2 - 3");
        Assert.Equal(p.Statements.Count, 1);
        _ = Assert.IsType<LetNode>(p.Statements[0]);

        var let = p.Statements[0].Cast<LetNode>();
        Assert.Equal(let.Var.Name, "a");

        _ = Assert.IsType<FunctionCallNode>(let.Expression);
        var minus = let.Expression.Cast<FunctionCallNode>();
        _ = Assert.IsType<FunctionCallNode>(minus.Arguments[0]);
        var plus = minus.Arguments[0].Cast<FunctionCallNode>();

        _ = Assert.IsType<VariableNode>(plus.Expression);
        _ = Assert.IsType<VariableNode>(minus.Expression);
        _ = Assert.IsType<NumericNode>(plus.Arguments[0]);
        _ = Assert.IsType<NumericNode>(plus.Arguments[1]);
        _ = Assert.IsType<NumericNode>(minus.Arguments[1]);

        Assert.Equal(plus.Expression.Cast<VariableNode>().Name, "+");
        Assert.Equal(minus.Expression.Cast<VariableNode>().Name, "-");
        Assert.Equal(plus.Arguments[0].Cast<NumericNode>().Value, 1u);
        Assert.Equal(plus.Arguments[1].Cast<NumericNode>().Value, 2u);
        Assert.Equal(minus.Arguments[1].Cast<NumericNode>().Value, 3u);
    }

    [Fact]
    public void CallTest()
    {
        var p = Parse("print(\"hello world\")");
        Assert.Equal(p.Statements.Count, 1);
        _ = Assert.IsType<FunctionCallNode>(p.Statements[0]);

        var call = p.Statements[0].Cast<FunctionCallNode>();
        _ = Assert.IsType<VariableNode>(call.Expression);
        var f = call.Expression.Cast<VariableNode>();
        Assert.Equal(f.Name, "print");

        Assert.Equal(call.Arguments.Count, 1);
        _ = Assert.IsType<StringNode>(call.Arguments[0]);
        var str = call.Arguments[0].Cast<StringNode>();
        Assert.Equal(str.Value, "hello world");
    }

    [Fact]
    public void FunctionTest()
    {
        var p = Parse(@"
sub fn(s: String, n: Int)
    var x = 12
");
        Assert.Equal(p.Statements.Count, 0);
        Assert.Equal(p.Functions.Count, 1);

        var fn = p.Functions[0];
        Assert.Equal(fn.Name.Name, "fn");
        Assert.Equal(fn.Arguments.Count, 2);
        Assert.Equal(fn.Arguments[0].Name.Name, "s");
        Assert.Equal(fn.Arguments[0].Type.Name, "String");
        Assert.Equal(fn.Arguments[1].Name.Name, "n");
        Assert.Equal(fn.Arguments[1].Type.Name, "Int");

        Assert.Equal(fn.Statements.Count, 1);
        _ = Assert.IsType<LetNode>(fn.Statements[0]);
        var let = fn.Statements[0].Cast<LetNode>();
        Assert.Equal(let.Var.Name, "x");
        _ = Assert.IsType<NumericNode>(let.Expression);

        var num = let.Expression.Cast<NumericNode>();
        Assert.Equal(num.Format, "12");
        Assert.Equal(num.Value, 12u);
    }

    [Fact]
    public void ContinueEolTest()
    {
        var p = Parse(@"
sub fn(
        s: String,
        n: Int,
    )
    var x = 12
");
        Assert.Equal(p.Statements.Count, 0);
        Assert.Equal(p.Functions.Count, 1);

        var fn = p.Functions[0];
        Assert.Equal(fn.Name.Name, "fn");
        Assert.Equal(fn.Arguments.Count, 2);
        Assert.Equal(fn.Arguments[0].Name.Name, "s");
        Assert.Equal(fn.Arguments[0].Type.Name, "String");
        Assert.Equal(fn.Arguments[1].Name.Name, "n");
        Assert.Equal(fn.Arguments[1].Type.Name, "Int");

        Assert.Equal(fn.Statements.Count, 1);
        _ = Assert.IsType<LetNode>(fn.Statements[0]);
        var let = fn.Statements[0].Cast<LetNode>();
        Assert.Equal(let.Var.Name, "x");
        _ = Assert.IsType<NumericNode>(let.Expression);

        var num = let.Expression.Cast<NumericNode>();
        Assert.Equal(num.Format, "12");
        Assert.Equal(num.Value, 12u);
    }

    [Fact]
    public void TypeParameterTest()
    {
        var p = Parse("var e = ListIndex<a>()");
        Assert.Equal(p.Statements.Count, 1);
        _ = Assert.IsType<LetNode>(p.Statements[0]);
        var let = p.Statements[0].Cast<LetNode>();
        Assert.Equal(let.Var.Name, "e");
        _ = Assert.IsType<FunctionCallNode>(let.Expression);

        var fcall = let.Expression.Cast<FunctionCallNode>();
        Assert.Equal(fcall.Arguments.Count, 0);
        _ = Assert.IsType<SpecializationNode>(fcall.Expression);
        var sp = fcall.Expression.Cast<SpecializationNode>();
        Assert.Equal(sp.Name, "ListIndex");
        Assert.Equal(sp.Generics.Count, 1);
        _ = Assert.IsType<TypeNode>(sp.Generics[0]);

        var type = sp.Generics[0].Cast<TypeNode>();
        Assert.Equal(type.Name, "a");
    }

    public static ProgramNode Parse(string s)
    {
        var lex = LexerTest.Read(s);
        return lex.Parser.Parse(lex).Cast<ProgramNode>();
    }
}
