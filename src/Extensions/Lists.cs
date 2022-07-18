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
        for (var i = 0; i < length; start++)
        {
            yield return start;
            if (++i >= length) break;
        }
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
    public static T? FirstOrNullValue<T>(this IEnumerable<T> self) where T : struct => self.IsEmpty() ? null : self.First().Cast<T?>();

    [DebuggerHidden]
    public static T? LastOrNullValue<T>(this IEnumerable<T> self) where T : struct => self.IsEmpty() ? null : self.Last().Cast<T?>();

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
    public static R FoldLeft<T, R>(this IEnumerable<T> self, Func<R, T, R> f, R acc) => self.IsEmpty() ? acc : self.Accumulator(f, acc).Last();

    [DebuggerHidden]
    public static R FoldRight<T, R>(this IEnumerable<T> self, Func<T, R, R> f, R acc) => self.IsEmpty() ? acc : self.Reverse().Accumulator((x, a) => f(a, x), acc).Last();

    [DebuggerHidden]
    public static T FoldLeft<T>(this IEnumerable<T> self, Func<T, T, T> f) => self.Accumulator(f).Last();

    [DebuggerHidden]
    public static T FoldRight<T>(this IEnumerable<T> self, Func<T, T, T> f) => self.Reverse().Accumulator((x, a) => f(a, x)).Last();

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
    public static IEnumerable<IEnumerable<T>> SplitBefore<T>(this IEnumerable<T> self, Func<T, bool> f)
    {
        var list = self.ToList();
        var start = 0;
        var skip = 0;
        while (true)
        {
            var index = list.FindIndex(skip, x => f(x));
            if (index < 0)
            {
                yield return list.GetRange(start, list.Count - start);
                break;
            }
            else
            {
                yield return list.GetRange(start, index - start);
                start = index;
                skip = index + 1;
            }
        }
    }

    [DebuggerHidden]
    public static (IEnumerable<T1> First, IEnumerable<T2> Second) UnZip<T1, T2>(this IEnumerable<(T1, T2)> self) => (self.Select(x => x.Item1), self.Select(x => x.Item2));

    [DebuggerHidden]
    public static T FindFirst<T>(this IEnumerable<T> self, Func<T, bool> f) => self.Where(f).First();

    [DebuggerHidden]
    public static T FindFirst<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.Where(f).First();

    [DebuggerHidden]
    public static T? FindFirstOrNull<T>(this IEnumerable<T> self, Func<T, bool> f) where T : class => self.Where(f).If<IEnumerable<T>, T?>(IsEmpty, _ => null, x => x.First());

    [DebuggerHidden]
    public static T? FindFirstOrNull<T>(this IEnumerable<T> self, Func<T, int, bool> f) where T : class => self.Where(f).If<IEnumerable<T>, T?>(IsEmpty, _ => null, x => x.First());

    [DebuggerHidden]
    public static T? FindFirstOrNullValue<T>(this IEnumerable<T> self, Func<T, bool> f) where T : struct => self.Where(f).If<IEnumerable<T>, T?>(IsEmpty, _ => null, x => x.First());

    [DebuggerHidden]
    public static T? FindFirstOrNullValue<T>(this IEnumerable<T> self, Func<T, int, bool> f) where T : struct => self.Where(f).If<IEnumerable<T>, T?>(IsEmpty, _ => null, x => x.First());

    [DebuggerHidden]
    public static int FindFirstIndex<T>(this IEnumerable<T> self, Func<T, bool> f) => self.Zip(Sequence(0)).Where(x => f(x.First)).If(IsEmpty, _ => -1, x => x.First().Second);

    [DebuggerHidden]
    public static int FindFirstIndex<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.Zip(Sequence(0)).Where(x => f(x.First, x.Second)).If(IsEmpty, _ => -1, x => x.First().Second);

    [DebuggerHidden]
    public static T FindLast<T>(this IEnumerable<T> self, Func<T, bool> f) => self.Reverse().Where(f).First();

    [DebuggerHidden]
    public static T FindLast<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.Zip(Sequence(0)).Reverse().Where(x => f(x.First, x.Second)).First().First;

    [DebuggerHidden]
    public static T? FindLastOrNull<T>(this IEnumerable<T> self, Func<T, bool> f) where T : class => self.Reverse().Where(f).If<IEnumerable<T>, T?>(IsEmpty, _ => null, x => x.First());

    [DebuggerHidden]
    public static T? FindLastOrNull<T>(this IEnumerable<T> self, Func<T, int, bool> f) where T : class => self.Zip(Sequence(0)).Reverse().Where(x => f(x.First, x.Second)).If<IEnumerable<(T, int)>, T?>(IsEmpty, _ => null, x => x.First().Item1);

    [DebuggerHidden]
    public static T? FindLastOrNullValue<T>(this IEnumerable<T> self, Func<T, bool> f) where T : struct => self.Reverse().Where(f).If<IEnumerable<T>, T?>(IsEmpty, _ => null, x => x.First());

    [DebuggerHidden]
    public static T? FindLastOrNullValue<T>(this IEnumerable<T> self, Func<T, int, bool> f) where T : struct => self.Zip(Sequence(0)).Reverse().Where(x => f(x.First, x.Second)).If<IEnumerable<(T, int)>, T?>(IsEmpty, _ => null, x => x.First().Item1);

    [DebuggerHidden]
    public static int FindLastIndex<T>(this IEnumerable<T> self, Func<T, bool> f) => self.Zip(Sequence(0)).Reverse().Where(x => f(x.First)).If(IsEmpty, _ => -1, x => x.First().Second);

    [DebuggerHidden]
    public static int FindLastIndex<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.Zip(Sequence(0)).Reverse().Where(x => f(x.First, x.Second)).If(IsEmpty, _ => -1, x => x.First().Second);

    [DebuggerHidden]
    public static bool Contains<T>(this IEnumerable<T> self, Func<T, bool> f) => self.FindFirstIndex(f) >= 0;

    [DebuggerHidden]
    public static bool Contains<T>(this IEnumerable<T> self, Func<T, int, bool> f) => self.FindFirstIndex(f) >= 0;

    [DebuggerHidden]
    public static T Sum<T>(this IEnumerable<T> self) => self.FoldLeft((x, y) => Expressions.Add<T>()(x, y));

    [DebuggerHidden]
    public static T Max<T>(this IEnumerable<T> self) where T : IComparable<T> => self.FoldLeft((x, y) => x.CompareTo(y) >= 0 ? x : y);

    [DebuggerHidden]
    public static T Min<T>(this IEnumerable<T> self) where T : IComparable<T> => self.FoldLeft((x, y) => x.CompareTo(y) <= 0 ? x : y);

    [DebuggerHidden]
    public static T Max<T, R>(this IEnumerable<T> self, Func<T, R> f) where R : IComparable<R> => self.Select(x => (x, f(x))).FoldLeft((x, y) => x.Item2.CompareTo(y.Item2) >= 0 ? x : y).x;

    [DebuggerHidden]
    public static T Min<T, R>(this IEnumerable<T> self, Func<T, R> f) where R : IComparable<R> => self.Select(x => (x, f(x))).FoldLeft((x, y) => x.Item2.CompareTo(y.Item2) <= 0 ? x : y).x;

    [DebuggerHidden]
    public static List<T> Sort<T>(this IEnumerable<T> self) where T : IComparable<T> => self.ToList().Return(x => x.Sort());

    [DebuggerHidden]
    public static List<T> Sort<T>(this IEnumerable<T> self, Func<T, T, int> f) => self.ToList().Return(x => x.Sort(new Comparison<T>(f)));

    [DebuggerHidden]
    public static IOrderedEnumerable<T> StableSort<T>(this IEnumerable<T> self) where T : IComparable<T> => Enumerable.OrderBy(self, x => x);

    [DebuggerHidden]
    public static IOrderedEnumerable<T> StableSort<T>(this IEnumerable<T> self, Func<T, T, int> f) => Enumerable.OrderBy(self, x => x, new ComparerBinder<T>() { Compare = f });
}
