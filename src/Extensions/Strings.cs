using System.Diagnostics;
using System.Text;

namespace Extensions
{
    public static class Strings
    {
        [DebuggerHidden]
        public static string SubstringAsByte(this string self, int length, Encoding enc) =>
            self.Substring(0,
                self.Map(x => enc.GetByteCount(new char[] { x })).
                Accumulator((acc, x) => acc + x).
                Take(x => x <= length).
                Count());
    }
}
