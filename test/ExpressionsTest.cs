using Extensions;
using NUnit.Framework;

namespace Roku.Tests;

public class ExpressionsTest
{
    [Test]
    public void AddTest()
    {
        var add_int = Expressions.Add<int>();
        Assert.AreEqual(add_int(1, 2), 3);

        var add_double = Expressions.Add<double>();
        var d = add_double(1.1, 2.2);
        Assert.IsTrue(d >= 3.30);
        Assert.IsTrue(d <= 3.31);
    }

    [Test]
    public void SubTest()
    {
        var sub_int = Expressions.Subtract<int>();
        Assert.AreEqual(sub_int(1, 2), -1);
    }

    [Test]
    public void MulTest()
    {
        var mul_int = Expressions.Multiply<int>();
        Assert.AreEqual(mul_int(3, 4), 12);
    }

    [Test]
    public void DivTest()
    {
        var div_int = Expressions.Divide<int>();
        Assert.AreEqual(div_int(7, 3), 2);
    }

    [Test]
    public void ModTest()
    {
        var mod_int = Expressions.Modulo<int>();
        Assert.AreEqual(mod_int(8, 3), 2);
    }

    [Test]
    public void LShiftTest()
    {
        var lshift_int = Expressions.LeftShift<int>();
        Assert.AreEqual(lshift_int(5, 3), 40);
    }

    [Test]
    public void RShiftTest()
    {
        var rshift_int = Expressions.RightShift<int>();
        Assert.AreEqual(rshift_int(40, 3), 5);
    }
}
