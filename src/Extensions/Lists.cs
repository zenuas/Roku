using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Extensions
{
    public static class Lists
    {
        [DebuggerHidden]
        public static IEnumerable<int> Sequence(int first, int tolerance = 1)
        {
            for (; ; first += tolerance) yield return first;
        }

        [DebuggerHidden]
        public static IEnumerable<int> Range(int start, int length) => Enumerable.Range(start, length);

        [DebuggerHidden]
        public static IEnumerable<char> Range(char start, int length)
        {
            for (var i = 0; i < length; start++)
            {
                yield return start;
                if (++i >= length) break;
            }
        }

        [DebuggerHidden]
        public static IEnumerable<int> RangeTo(int start, int end) => Enumerable.Range(start, end - start + 1);

        [DebuggerHidden]
        public static IEnumerable<char> RangeTo(char start, char end)
        {
            for (var c = start; ; c++)
            {
                yield return c;
                if (c >= end) break;
            }
        }

        [DebuggerHidden]
        public static IEnumerable<T> Repeat<T>(T x)
        {
            for (; ; ) yield return x;
        }

        [DebuggerHidden]
        public static int Length<T>(this IEnumerable<T> self) => Enumerable.Count(self);

        [DebuggerHidden]
        public static bool IsNull<T>(this IEnumerable<T> self) => !self.Any();

        [DebuggerHidden]
        public static T First<T>(this IEnumerable<T> self) => Enumerable.First(self);

        [DebuggerHidden]
        public static T? FirstOrNull<T>(this IEnumerable<T> self) where T : class => Enumerable.FirstOrDefault(self);

        [DebuggerHidden]
        public static T? FirstOrNullValue<T>(this IEnumerable<T> self) where T : struct => self.IsNull() ? null : self.First().Cast<T?>();

        [DebuggerHidden]
        public static IEnumerable<T> Next<T>(this IEnumerable<T> self) => !self.IsNull() ? self.Drop(1) : throw new IndexOutOfRangeException();

        [DebuggerHidden]
        public static void Each<T>(this IEnumerable<T> self, Action<T> f)
        {
            foreach (var v in self) f(v);
        }

        [DebuggerHidden]
        public static void Each<T>(this IEnumerable<T> self, Action<T, int> f) => self.Zip(Sequence(0)).Each(x => f(x.First, x.Second));

        [DebuggerHidden]
        public static IEnumerable<T> Apply<T>(this IList<T> self, Func<T, T> f)
        {
            for (var i = 0; i < self.Count; i++)
            {
                yield return self[i] = f(self[i]);
            }
        }

        [DebuggerHidden]
        public static R FoldLeft<T, R>(this IEnumerable<T> self, Func<R, T, R> f, R acc)
        {
            self.Each(x => acc = f(acc, x));
            return acc;
        }

        [DebuggerHidden]
        public static R FoldRight<T, R>(this IEnumerable<T> self, Func<T, R, R> f, R acc)
        {
            self.Reverse().Each(x => acc = f(x, acc));
            return acc;
        }

        [DebuggerHidden]
        public static T FoldLeft<T>(this IEnumerable<T> self, Func<T, T, T> f)
        {
            var acc = self.First();
            self.Drop(1).Each(x => acc = f(acc, x));
            return acc;
        }

        [DebuggerHidden]
        public static T FoldRight<T>(this IEnumerable<T> self, Func<T, T, T> f)
        {
            var rev = self.Reverse();
            var acc = rev.First();
            rev.Drop(1).Each(x => acc = f(x, acc));
            return acc;
        }

        [DebuggerHidden]
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> self, IEnumerable<T> xs) => Enumerable.Concat(self, xs);

        [DebuggerHidden]
        public static bool SequenceEqual<T>(this IEnumerable<T> self, IEnumerable<T> xs) => Enumerable.SequenceEqual(self, xs);

        [DebuggerHidden]
        public static bool And<T>(this IEnumerable<T> self, Func<T, bool> f) => Enumerable.All(self, f);

        [DebuggerHidden]
        public static bool And<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.Zip(Sequence(0)).All(x => f(x.First, x.Second));

        [DebuggerHidden]
        public static bool Or<T>(this IEnumerable<T> self, Func<T, bool> f) => Enumerable.Any(self, f);

        [DebuggerHidden]
        public static bool Or<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.Zip(Sequence(0)).Any(x => f(x.First, x.Second));

        [DebuggerHidden]
        public static IEnumerable<R> Map<T, R>(this IEnumerable<T> self, Func<T, R> f) => self.Select(f);

        [DebuggerHidden]
        public static IEnumerable<R> Map<T, R>(this IEnumerable<T> self, Func<T, int, R> f) => self.Select(f);

        [DebuggerHidden]
        public static IEnumerable<T> Where<T>(this IEnumerable<T> self, Func<T, bool> f) => Enumerable.Where(self, f);

        [DebuggerHidden]
        public static IEnumerable<T> Where<T>(this IEnumerable<T> self, Func<T, int, bool> f) => Enumerable.Where(self, f);

        [DebuggerHidden]
        public static IEnumerable<T> Take<T>(this IEnumerable<T> self, int count) => Enumerable.Take(self, count);

        [DebuggerHidden]
        public static IEnumerable<T> Drop<T>(this IEnumerable<T> self, int count) => Enumerable.Skip(self, count);

        [DebuggerHidden]
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> self) => self.SelectMany(x => x);

        [DebuggerHidden]
        public static IEnumerable<(T1 First, T2 Second)> Zip<T1, T2>(this IEnumerable<T1> self, IEnumerable<T2> xs) => Enumerable.Zip(self, xs);

        [DebuggerHidden]
        public static (IEnumerable<T1> First, IEnumerable<T2> Second) UnZip<T1, T2>(this IEnumerable<(T1, T2)> self) => (self.Map(x => x.Item1), self.Map(x => x.Item2));

        [DebuggerHidden]
        public static IEnumerable<T> Reverse<T>(this IEnumerable<T> self) => Enumerable.Reverse(self);

        [DebuggerHidden]
        public static IEnumerable<T> Unique<T>(this IEnumerable<T> self) => Enumerable.Distinct(self);

        [DebuggerHidden]
        public static IEnumerable<T> By<T>(this System.Collections.IEnumerable self) => Enumerable.OfType<T>(self);

        [DebuggerHidden]
        public static IEnumerable<R> By<R, T>(this IEnumerable<T> self) => Enumerable.OfType<R>(self);

        [DebuggerHidden]
        public static T FindFirst<T>(this IEnumerable<T> self, Func<T, bool> f) => self.Where(f).First();

        [DebuggerHidden]
        public static T FindFirst<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.Where(f).First();

        [DebuggerHidden]
        public static T? FindFirstOrNull<T>(this IEnumerable<T> self, Func<T, bool> f) where T : class => self.Where(f).If<IEnumerable<T>, T?>(x => x.IsNull(), _ => null, x => x.First());

        [DebuggerHidden]
        public static T? FindFirstOrNull<T>(this IEnumerable<T> self, Func<T, int, bool> f) where T : class => self.Where(f).If<IEnumerable<T>, T?>(x => x.IsNull(), _ => null, x => x.First());

        [DebuggerHidden]
        public static T? FindFirstOrNullValue<T>(this IEnumerable<T> self, Func<T, bool> f) where T : struct => self.Where(f).If<IEnumerable<T>, T?>(x => x.IsNull(), _ => null, x => x.First());

        [DebuggerHidden]
        public static T? FindFirstOrNullValue<T>(this IEnumerable<T> self, Func<T, int, bool> f) where T : struct => self.Where(f).If<IEnumerable<T>, T?>(x => x.IsNull(), _ => null, x => x.First());

        [DebuggerHidden]
        public static int FindFirstIndex<T>(this IEnumerable<T> self, Func<T, bool> f) => self.Zip(Sequence(0)).Where(x => f(x.First)).If(IsNull, _ => -1, x => x.First().Second);

        [DebuggerHidden]
        public static int FindFirstIndex<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.Zip(Sequence(0)).Where(x => f(x.First, x.Second)).If(IsNull, _ => -1, x => x.First().Second);

        [DebuggerHidden]
        public static T FindLast<T>(this IEnumerable<T> self, Func<T, bool> f) => self.Reverse().Where(f).First();

        [DebuggerHidden]
        public static T FindLast<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.Zip(Sequence(0)).Reverse().Where(x => f(x.First, x.Second)).First().First;

        [DebuggerHidden]
        public static T? FindLastOrNull<T>(this IEnumerable<T> self, Func<T, bool> f) where T : class => self.Reverse().Where(f).If<IEnumerable<T>, T?>(x => x.IsNull(), _ => null, x => x.First());

        [DebuggerHidden]
        public static T? FindLastOrNull<T>(this IEnumerable<T> self, Func<T, int, bool> f) where T : class => self.Zip(Sequence(0)).Reverse().Where(x => f(x.First, x.Second)).If<IEnumerable<(T, int)>, T?>(x => x.IsNull(), _ => null, x => x.First().Item1);

        [DebuggerHidden]
        public static T? FindLastOrNullValue<T>(this IEnumerable<T> self, Func<T, bool> f) where T : struct => self.Reverse().Where(f).If<IEnumerable<T>, T?>(x => x.IsNull(), _ => null, x => x.First());

        [DebuggerHidden]
        public static T? FindLastOrNullValue<T>(this IEnumerable<T> self, Func<T, int, bool> f) where T : struct => self.Zip(Sequence(0)).Reverse().Where(x => f(x.First, x.Second)).If<IEnumerable<(T, int)>, T?>(x => x.IsNull(), _ => null, x => x.First().Item1);

        [DebuggerHidden]
        public static int FindLastIndex<T>(this IEnumerable<T> self, Func<T, bool> f) => self.Zip(Sequence(0)).Reverse().Where(x => f(x.First)).If(IsNull, _ => -1, x => x.First().Second);

        [DebuggerHidden]
        public static int FindLastIndex<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.Zip(Sequence(0)).Reverse().Where(x => f(x.First, x.Second)).If(IsNull, _ => -1, x => x.First().Second);

        [DebuggerHidden]
        public static T Max<T>(this IEnumerable<T> self) where T : IComparable<T> => self.FoldLeft((x, y) => x.CompareTo(y) >= 0 ? x : y);

        [DebuggerHidden]
        public static T Min<T>(this IEnumerable<T> self) where T : IComparable<T> => self.FoldLeft((x, y) => x.CompareTo(y) <= 0 ? x : y);

        [DebuggerHidden]
        public static T Max<T, R>(this IEnumerable<T> self, Func<T, R> f) where R : IComparable<R> => self.Map(x => (x, f(x))).FoldLeft((x, y) => x.Item2.CompareTo(y.Item2) >= 0 ? x : y).x;

        [DebuggerHidden]
        public static T Min<T, R>(this IEnumerable<T> self, Func<T, R> f) where R : IComparable<R> => self.Map(x => (x, f(x))).FoldLeft((x, y) => x.Item2.CompareTo(y.Item2) <= 0 ? x : y).x;

        [DebuggerHidden]
        public static List<T> Sort<T>(this IEnumerable<T> self) where T : IComparable<T> => self.ToList().Return(x => x.Sort());

        [DebuggerHidden]
        public static List<T> Sort<T>(this IEnumerable<T> self, Func<T, T, int> f) => self.ToList().Return(x => x.Sort(new Comparison<T>(f)));

        [DebuggerHidden]
        public static IOrderedEnumerable<T> StableSort<T>(this IEnumerable<T> self) where T : IComparable<T> => Enumerable.OrderBy(self, x => x);

        [DebuggerHidden]
        public static IOrderedEnumerable<T> StableSort<T>(this IEnumerable<T> self, Func<T, T, int> f) => Enumerable.OrderBy(self, x => x, new ComparerBinder<T>() { Compare = f });

        [DebuggerHidden]
        public static List<T> ToList<T>(this IEnumerable<T> self) => Enumerable.ToList(self);

        [DebuggerHidden]
        public static T[] ToArray<T>(this IEnumerable<T> self) => Enumerable.ToArray(self);

        [DebuggerHidden]
        public static string Join(this IEnumerable<string> self, char separator) => string.Join(separator, self);

        [DebuggerHidden]
        public static string Join(this IEnumerable<string> self, string separator = "") => string.Join(separator, self);

        [DebuggerHidden]
        public static Dictionary<K, T> ToDictionary<T, K>(this IEnumerable<T> self, Func<T, K> fkey) where K : notnull => Enumerable.ToDictionary(self, fkey);

        [DebuggerHidden]
        public static Dictionary<K, V> ToDictionary<T, K, V>(this IEnumerable<T> self, Func<T, K> fkey, Func<T, V> fvalue) where K : notnull => Enumerable.ToDictionary(self, fkey, fvalue);

        [DebuggerHidden]
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> self) => Enumerable.ToHashSet(self);

        [DebuggerHidden]
        public static string ToStringByChars(this IEnumerable<char> self) => new string(self.ToArray());
    }
}
