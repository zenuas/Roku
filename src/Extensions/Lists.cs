using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Extensions;

public static class Lists
{
    [DebuggerHidden]
    public static IEnumerable<int> Sequence(int first, int tolerance = 1)
    {
        for (; ; first += tolerance) yield return first;
    }

    [DebuggerHidden]
    public static IEnumerable<char> Range(char start, int length)
    {
        for (var i = 0; i < length; i++, start++) yield return start;
    }

    [DebuggerHidden]
    public static IEnumerable<int> RangeTo(int start, int end) => Enumerable.Range(start, end - start + 1);

    [DebuggerHidden]
    public static IEnumerable<char> RangeTo(char start, char end) => Range(start, end - start + 1);

    [DebuggerHidden]
    public static IEnumerable<T> Repeat<T>(T x)
    {
        for (; ; ) yield return x;
    }

    [DebuggerHidden]
    public static bool IsEmpty<T>(this IEnumerable<T> self) => !self.Any();

    [DebuggerHidden]
    public static IEnumerable<T> Next<T>(this IEnumerable<T> self) => !self.IsEmpty() ? self.Skip(1) : throw new IndexOutOfRangeException();

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
    public static IEnumerable<R> Accumulator<T, R>(this IEnumerable<T> self, Func<R, T, R> f, R acc)
    {
        foreach (var v in self) yield return acc = f(acc, v);
    }

    [DebuggerHidden]
    public static IEnumerable<T> Accumulator<T>(this IEnumerable<T> self, Func<T, T, T> f)
    {
        if (self.IsEmpty()) yield break;
        var acc = self.First();
        yield return acc;
        foreach (var v in self.Skip(1)) yield return acc = f(acc, v);
    }

    [DebuggerHidden]
    public static IEnumerable<T> Concat<T>(this IEnumerable<T> self, T x) => Enumerable.Concat(self, new T[] { x });

    [DebuggerHidden]
    public static IEnumerable<Task<R>> MapParallel<T, R>(this IEnumerable<T> self, Func<T, R> f) => self.Select(x => Task.Run(() => f(x)));

    [DebuggerHidden]
    public static IEnumerable<Task<R>> MapParallel<T, R>(this IEnumerable<T> self, Func<T, int, R> f) => self.Select((x, i) => Task.Run(() => f(x, i)));

    [DebuggerHidden]
    public static R[] MapParallelAll<T, R>(this IEnumerable<T> self, Func<T, R> f) => Task.WhenAll(self.MapParallel(f)).Result;

    [DebuggerHidden]
    public static R[] MapParallelAll<T, R>(this IEnumerable<T> self, Func<T, int, R> f) => Task.WhenAll(self.MapParallel(f)).Result;

    [DebuggerHidden]
    public static IEnumerable<(bool Completed, R? Result)> MapParallelAllWithTimeout<T, R>(this IEnumerable<T> self, Func<T, R> f, int waitms) => self.MapParallelAllWithTimeout(f, waitms, _ => default);

    [DebuggerHidden]
    public static IEnumerable<(bool Completed, R? Result)> MapParallelAllWithTimeout<T, R>(this IEnumerable<T> self, Func<T, R> f, int waitms, Func<T, R?> error)
    {
        var xs = self.ToList();
        var tasks = xs.MapParallel(f).ToArray();
        _ = Task.WaitAll(tasks, waitms);
        return tasks.Select((x, i) => (x.IsCompleted, x.IsCompleted ? x.Result : error(xs[i])));
    }

    [DebuggerHidden]
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> self) => self.SelectMany(x => x);

    [DebuggerHidden]
    public static IEnumerable<(IEnumerable<T> Values, bool Found, T Separator)> SplitFor<T>(this IEnumerable<T> self, Func<T, bool> f, bool prepend = false)
    {
        var values = new List<T>();
        foreach (var x in self)
        {
            if (f(x))
            {
                yield return (values.ToArray(), true, x);
                values.Clear();
                if (prepend) values.Add(x);
            }
            else
            {
                values.Add(x);
            }
        }
        yield return (values, false, default!);
    }

    [DebuggerHidden]
    public static IEnumerable<IEnumerable<T>> SplitIn<T>(this IEnumerable<T> self, Func<T, bool> f) => self.SplitFor(f).Select(x => x.Values);

    [DebuggerHidden]
    public static IEnumerable<IEnumerable<T>> SplitBefore<T>(this IEnumerable<T> self, Func<T, bool> f) => self.SplitFor(f, true).Select(x => x.Values);

    [DebuggerHidden]
    public static IEnumerable<IEnumerable<T>> SplitAfter<T>(this IEnumerable<T> self, Func<T, bool> f) => self.SplitFor(f).Select(x => x.Found ? x.Values.Concat(x.Separator) : x.Values);

    [DebuggerHidden]
    public static (IEnumerable<T1> First, IEnumerable<T2> Second) UnZip<T1, T2>(this IEnumerable<(T1, T2)> self) => (self.Select(x => x.Item1), self.Select(x => x.Item2));

    [DebuggerHidden]
    public static int FindFirstIndex<T>(this IEnumerable<T> self, Func<T, bool> f) => self.Zip(Sequence(0)).Where(x => f(x.First)).If(IsEmpty, _ => -1, x => x.First().Second);

    [DebuggerHidden]
    public static int FindFirstIndex<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.Zip(Sequence(0)).Where(x => f(x.First, x.Second)).If(IsEmpty, _ => -1, x => x.First().Second);

    [DebuggerHidden]
    public static int FindLastIndex<T>(this IEnumerable<T> self, Func<T, bool> f) => self.Zip(Sequence(0)).Reverse().Where(x => f(x.First)).If(IsEmpty, _ => -1, x => x.First().Second);

    [DebuggerHidden]
    public static int FindLastIndex<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.Zip(Sequence(0)).Reverse().Where(x => f(x.First, x.Second)).If(IsEmpty, _ => -1, x => x.First().Second);

    [DebuggerHidden]
    public static T? FindFirstOrNullValue<T>(this IEnumerable<T> self, Func<T, bool> f) where T : struct => self.Where(f).If<IEnumerable<T>, T?>(IsEmpty, _ => null, x => x.First());

    [DebuggerHidden]
    public static T? FindFirstOrNullValue<T>(this IEnumerable<T> self, Func<T, int, bool> f) where T : struct => self.Where(f).If<IEnumerable<T>, T?>(IsEmpty, _ => null, x => x.First());

    [DebuggerHidden]
    public static T? FindLastOrNullValue<T>(this IEnumerable<T> self, Func<T, bool> f) where T : struct => self.Reverse().Where(f).If<IEnumerable<T>, T?>(IsEmpty, _ => null, x => x.First());

    [DebuggerHidden]
    public static T? FindLastOrNullValue<T>(this IEnumerable<T> self, Func<T, int, bool> f) where T : struct => self.Zip(Sequence(0)).Reverse().Where(x => f(x.First, x.Second)).If<IEnumerable<(T, int)>, T?>(IsEmpty, _ => null, x => x.First().Item1);

    [DebuggerHidden]
    public static bool Contains<T>(this IEnumerable<T> self, Func<T, bool> f) => self.FindFirstIndex(f) >= 0;

    [DebuggerHidden]
    public static bool Contains<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.FindFirstIndex(f) >= 0;

    [DebuggerHidden]
    public static IOrderedEnumerable<T> Order<T>(this IEnumerable<T> self, Func<T, T, int> f) => self.Order(new ComparerBinder<T> { Compare = f });

    [DebuggerHidden]
    public static IOrderedEnumerable<T> OrderBy<T, R>(this IEnumerable<T> self, Func<T, R> selector, Func<R, R, int> f) => self.OrderBy(selector, new ComparerBinder<R> { Compare = f });

    [DebuggerHidden]
    public static IOrderedEnumerable<T> OrderDescending<T>(this IEnumerable<T> self, Func<T, T, int> f) => self.OrderDescending(new ComparerBinder<T> { Compare = f });

    [DebuggerHidden]
    public static IOrderedEnumerable<T> OrderByDescending<T, R>(this IEnumerable<T> self, Func<T, R> selector, Func<R, R, int> f) => self.OrderByDescending(selector, new ComparerBinder<R> { Compare = f });

    [DebuggerHidden]
    public static IOrderedEnumerable<T> ThenBy<T, R>(this IOrderedEnumerable<T> self, Func<T, R> selector, Func<R, R, int> f) => self.ThenBy(selector, new ComparerBinder<R> { Compare = f });

    [DebuggerHidden]
    public static IOrderedEnumerable<T> ThenByDescending<T, R>(this IOrderedEnumerable<T> self, Func<T, R> selector, Func<R, R, int> f) => self.ThenByDescending(selector, new ComparerBinder<R> { Compare = f });

    [DebuggerHidden]
    public static IEnumerable<T> Distinct<T>(this IEnumerable<T> self, Func<T?, T?, bool> f) => Enumerable.Distinct(self, new EqualityComparerBinder<T>() { Equals = f });

    [DebuggerHidden]
    public static IEnumerable<T> Union<T>(this IEnumerable<T> self, IEnumerable<T> second, Func<T?, T?, bool> f) => Enumerable.Union(self, second, new EqualityComparerBinder<T>() { Equals = f });

    [DebuggerHidden]
    public static IEnumerable<T> Intersect<T>(this IEnumerable<T> self, IEnumerable<T> second, Func<T?, T?, bool> f) => Enumerable.Intersect(self, second, new EqualityComparerBinder<T>() { Equals = f });

    [DebuggerHidden]
    public static IEnumerable<T> Except<T>(this IEnumerable<T> self, IEnumerable<T> second, Func<T?, T?, bool> f) => Enumerable.Except(self, second, new EqualityComparerBinder<T>() { Equals = f });
}
