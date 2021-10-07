using System.Diagnostics;
using System.Text;

namespace Extensions
{
    public static class Strings
    {
        [DebuggerHidden]
        public static int CountAsByte(this string self, int length, Encoding enc) =>
            self.Map(x => enc.GetByteCount(new char[] { x })).
                Accumulator((acc, x) => acc + x).
                Take(x => x <= length).
                Count();

        [DebuggerHidden]
        public static string SubstringAsByte(this string self, int startIndex, Encoding enc) => self.Substring(self.CountAsByte(startIndex, enc));

        [DebuggerHidden]
        public static string SubstringAsByte(this string self, int startIndex, int length, Encoding enc) => self.SubstringAsByte(startIndex, enc).To(x => x.Substring(0, x.CountAsByte(length, enc)));
    }
}
