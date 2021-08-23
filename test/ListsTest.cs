using Extensions;
using NUnit.Framework;
using System;
using System.Threading;

namespace Roku.Tests
{
    public class ListsTest
    {
        [Test]
        public void SequenceTest()
        {
            var xs = Lists.Sequence(1);
            var ys = Lists.RangeTo('a', 'f');
            var zs = xs.Zip(ys);
            var (a, b) = zs.UnZip();
            Assert.IsTrue(xs.Take(ys.Length()).SequenceEqual(a));
            Assert.IsTrue(ys.SequenceEqual(b));
        }

        [Test]
        public void RangeTest()
        {
            var xs = Lists.Range(int.MaxValue - 5, 6);
            Assert.IsTrue(xs.SequenceEqual(new int[] {
                int.MaxValue - 5,
                int.MaxValue - 4,
                int.MaxValue - 3,
                int.MaxValue - 2,
                int.MaxValue - 1,
                int.MaxValue }));

            var ys = Lists.Range((char)(char.MaxValue - 5), 6);
            Assert.IsTrue(ys.SequenceEqual(new char[] {
                (char)(char.MaxValue - 5),
                (char)(char.MaxValue - 4),
                (char)(char.MaxValue - 3),
                (char)(char.MaxValue - 2),
                (char)(char.MaxValue - 1),
                char.MaxValue }));

            Assert.IsTrue(Lists.Range(0, 0).SequenceEqual(new int[] { }));
            Assert.IsTrue(Lists.Range('A', 0).SequenceEqual(new char[] { }));

            _ = Assert.Throws<ArgumentOutOfRangeException>(() => Lists.Range(0, -1));
        }

        [Test]
        public void RangeToTest()
        {
            var xs = Lists.RangeTo('a', 'z');
            var ys = Lists.RangeTo('A', 'Z');
            var zs = new (char, char)[] { ('a', 'z'), ('A', 'Z') }.Map(x => Lists.RangeTo(x.Item1, x.Item2)).Flatten();
            Assert.IsTrue(zs.SequenceEqual(xs.Concat(ys)));
        }

        [Test]
        public void NextTest()
        {
            var xs = new int[] { };
            var ys = new int[] { 1, 2, 3 };
            _ = Assert.Throws<IndexOutOfRangeException>(() => xs.Next());
            Assert.IsTrue(ys.Next().SequenceEqual(new int[] { 2, 3 }));
        }

        [Test]
        public void SortTest()
        {
            var xs = new (int, string)[] {
                (10, "a"),
                (11, "b"),
                (12, "c"),
                (11, "d"),
                (13, "e"),
                (10, "f"),
                (12, "g"),
                (14, "h"),
                (10, "i"),
                (11, "j"),
                (12, "k"),
                (11, "l"),
                (13, "m"),
                (10, "n"),
                (12, "o"),
                (14, "p"),
                (10, "q"),
            };

            var unstable_sort = xs.Sort((x, y) => x.Item1 - y.Item1);
            var stable_sort = xs.StableSort((x, y) => x.Item1 - y.Item1).ToList();

            Assert.IsTrue(!unstable_sort.SequenceEqual(stable_sort));

            Assert.IsTrue(stable_sort.SequenceEqual(new (int, string)[] {
                (10, "a"),
                (10, "f"),
                (10, "i"),
                (10, "n"),
                (10, "q"),
                (11, "b"),
                (11, "d"),
                (11, "j"),
                (11, "l"),
                (12, "c"),
                (12, "g"),
                (12, "k"),
                (12, "o"),
                (13, "e"),
                (13, "m"),
                (14, "h"),
                (14, "p"),
            }));
        }

        [Test]
        public void DropTest()
        {
            Assert.IsTrue("abc123".Drop(3).ToStringByChars() == "123");
            Assert.IsTrue("abc123"[0..^0].ToStringByChars() == "abc123");
            Assert.IsTrue("abc123"[0..^1].ToStringByChars() == "abc12");
            Assert.IsTrue("a"[0..^0].ToStringByChars() == "a");
            Assert.IsTrue("a"[0..^1].ToStringByChars() == "");
            Assert.IsTrue(""[0..^0].ToStringByChars() == "");
        }

        [Test]
        public void TimeoutTest()
        {
            var xs = new int[] { 1, 2, 3, 1000, 5 };
            var r = xs.MapParallelAllWithTimeout(x =>
            {
                Thread.Sleep(x);
                return $"{x}_{x}";
            }, 100).ToArray();

            Assert.IsTrue(r[0].Completed);
            Assert.IsTrue(r[0].Result == "1_1");
            Assert.IsTrue(r[1].Completed);
            Assert.IsTrue(r[1].Result == "2_2");
            Assert.IsTrue(r[2].Completed);
            Assert.IsTrue(r[2].Result == "3_3");
            Assert.IsTrue(!r[3].Completed);
            Assert.IsTrue(r[3].Result is null);
            Assert.IsTrue(r[4].Completed);
            Assert.IsTrue(r[4].Result == "5_5");
        }

        [Test]
        public void SplitBeforeTest()
        {
            var xs1 = new int[] { }.SplitBefore(x => x < 0).ToList();
            Assert.IsTrue(xs1.Count == 1);
            Assert.AreEqual(xs1[0], new int[] { });

            var xs2 = new int[] { 1, 2, 3, -1, 4, 5, -2, -3, 6, -4 }.SplitBefore(x => x < 0).ToList();
            Assert.IsTrue(xs2.Count == 5);
            Assert.AreEqual(xs2[0], new int[] { 1, 2, 3 });
            Assert.AreEqual(xs2[1], new int[] { -1, 4, 5 });
            Assert.AreEqual(xs2[2], new int[] { -2 });
            Assert.AreEqual(xs2[3], new int[] { -3, 6 });
            Assert.AreEqual(xs2[4], new int[] { -4 });
        }
    }
}
