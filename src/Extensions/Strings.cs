using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mina.Extensions;

public static class Strings
{
    [DebuggerHidden]
    public static int CountAsByte(this string self, int length, Encoding enc) =>
        self.Select(x => enc.GetByteCount([x]))
            .Accumulator((acc, x) => acc + x)
            .TakeWhile(x => x <= length)
            .Count();

    [DebuggerHidden]
    public static string SubstringAsByte(this string self, int startIndex, Encoding enc) => self[self.CountAsByte(startIndex, enc)..];

    [DebuggerHidden]
    public static string SubstringAsByte(this string self, int startIndex, int length, Encoding enc) => self.SubstringAsByte(startIndex, enc).To(x => x[..x.CountAsByte(length, enc)]);

    [DebuggerHidden]
    public static string Join(this IEnumerable<string> self, char separator) => string.Join(separator, self);

    [DebuggerHidden]
    public static string Join(this IEnumerable<string> self, string separator = "") => string.Join(separator, self);

    [DebuggerHidden]
    public static string[] SplitLine(this string self) => self.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);

    [DebuggerHidden]
    public static string ToStringByChars(this IEnumerable<char> self) => new([.. self]);
}
