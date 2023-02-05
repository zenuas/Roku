using System;
using System.Collections.Generic;

namespace Extensions;

public class ComparerBinder<T> : IComparer<T>
{
    public required Func<T, T, int> Compare;

    int IComparer<T>.Compare(T? x, T? y) => Compare(x!, y!);
}
