using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Extensions;

public class ComparerBinder<T> : IComparer<T>
{
    public Func<T, T, int>? Compare;

    int IComparer<T>.Compare([AllowNull] T x, [AllowNull] T y) => Compare!(x!, y!);
}
