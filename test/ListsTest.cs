using Extensions;
using NUnit.Framework;
using System;
using System.Diagnostics;

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

            var sw = new Stopwatch();
            sw.Restart();
            foreach (var x in Lists.Range(0, 10000))
            {
                xs.ToList().Sort(new Comparison<(int, string)>((x, y) => x.Item1 - y.Item1));
            }
            var rap1 = sw.ElapsedMilliseconds;
            sw.Restart();
            foreach (var x in Lists.Range(0, 10000))
            {
                var sort = xs.Sort((x, y) => x.Item1 - y.Item1);
            }
            var rap2 = sw.ElapsedMilliseconds;
            sw.Restart();
            foreach (var x in Lists.Range(0, 10000))
            {
                var sort = xs.StableSort((x, y) => x.Item1 - y.Item1).ToList();
            }
            var rap3 = sw.ElapsedMilliseconds;

            Assert.IsTrue(rap1 < rap2);
            Assert.IsTrue(rap2 < rap3);

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
        }
    }
}
