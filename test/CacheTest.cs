using Extensions;
using NUnit.Framework;
using System;

namespace Roku.Tests
{
    public class CacheTest
    {
        [Test]
        public void MemoTest()
        {
            var count = 0;
            int fib(int x)
            {
                count++;
                return x <= 1 ? 1 : fib(x - 1) + fib(x - 2);
            }

            count = 0;
            Assert.AreEqual(fib(0), 1);
            Assert.AreEqual(count, 1);

            count = 0;
            Assert.AreEqual(fib(1), 1);
            Assert.AreEqual(count, 1);

            count = 0;
            Assert.AreEqual(fib(2), 2);
            Assert.AreEqual(count, 3);

            count = 0;
            Assert.AreEqual(fib(3), 3);
            Assert.AreEqual(count, 5);

            count = 0;
            Assert.AreEqual(fib(4), 5);
            Assert.AreEqual(count, 9);

            count = 0;
            Assert.AreEqual(fib(10), 89);
            Assert.AreEqual(count, 177);

            Func<int, int> fibmemo = x => 1;
            int fib2(int x)
            {
                count++;
                return x <= 1 ? 1 : fibmemo(x - 1) + fibmemo(x - 2);
            }
            fibmemo = Cache.Memoization<int, int>(fib2);

            count = 0;
            Assert.AreEqual(fibmemo(0), 1);
            Assert.AreEqual(count, 1);

            count = 0;
            Assert.AreEqual(fibmemo(1), 1);
            Assert.AreEqual(count, 1);

            count = 0;
            Assert.AreEqual(fibmemo(2), 2);
            Assert.AreEqual(count, 1);

            count = 0;
            Assert.AreEqual(fibmemo(3), 3);
            Assert.AreEqual(count, 1);

            count = 0;
            Assert.AreEqual(fibmemo(4), 5);
            Assert.AreEqual(count, 1);

            count = 0;
            Assert.AreEqual(fibmemo(10), 89);
            Assert.AreEqual(count, 6);
        }
    }
}
