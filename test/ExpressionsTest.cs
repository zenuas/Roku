using Extensions;
using NUnit.Framework;

namespace Roku.Tests
{
    class ExpressionsTest
    {
        [Test]
        public void AddTest()
        {
            var add_int = Expressions.Add<int>();
            Assert.IsTrue(add_int(1, 2) == 3);

            var add_double = Expressions.Add<double>();
            var d = add_double(1.1, 2.2);
            Assert.IsTrue(d >= 3.30);
            Assert.IsTrue(d <= 3.31);
        }

    }
}
