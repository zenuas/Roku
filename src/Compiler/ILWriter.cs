using Extensions;
using System.IO;
using System.Text.RegularExpressions;

namespace Roku.Compiler
{
    public class ILWriter : StreamWriter
    {
        public int Indent { get; set; } = 0;
        public bool IsLineHead { get; protected set; } = true;

        public ILWriter(string path) : base(path)
        {
        }

        public override void WriteLine()
        {
            IsLineHead = WriteLine(new string[] { "" }, Indent, IsLineHead);
        }

        public override void WriteLine(string? s)
        {
            IsLineHead = WriteLine(s!.SplitLine(), Indent, IsLineHead);
        }

        public override void Write(string? s)
        {
            IsLineHead = Write(s!.SplitLine(), Indent, IsLineHead);
        }

        public bool Write(string[] lines, int indent, bool line_head = true)
        {
            var head = Lists.Repeat(" ").Take(4 * indent).Join("");
            if (lines.Length > 1)
            {
                lines[0..^1].Each((x, i) => base.WriteLine((i > 0 || line_head ? head : "") + x));
            }
            var last_write = (lines.Length > 1 || (lines.Length == 1 && line_head) ? head : "") + lines[^0];
            base.Write(last_write);
            return (lines.Length > 1 || (lines.Length == 1 && line_head)) && last_write == "";
        }

        public bool WriteLine(string[] lines, int indent, bool line_head = true)
        {
            lines.Each((x, i) => base.WriteLine((i > 0 || line_head ? Lists.Repeat(" ").Take(4 * indent).Join("") : "") + x));
            return true;
        }
    }
}
