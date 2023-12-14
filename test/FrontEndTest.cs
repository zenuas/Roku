using Extensions;
using NUnit.Framework;
using Roku.Compiler;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Roku.Tests;

public class FrontEndTest
{
    public static string SourceDir = Path.Combine("..", "..", "..", "..", "test-rk");

    [SetUp]
    public void Setup()
    {
        Trace.Listeners.Clear();
        _ = Trace.Listeners.Add(new NoFailListener());
    }

    public static (bool Found, string Text) GetLineContent(string[] lines, Func<string, bool> start_line, Func<string, bool> end_line)
    {
        var start = lines.FindFirstIndex(x => start_line(x));
        if (start < 0) return (false, "not found start_line");
        var end = lines.Skip(start + 1).FindFirstIndex(x => end_line(x));
        if (end < 0) return (false, "not found start_line - end_line");
        return (true, lines[(start + 1)..(start + end + 1)].Join("\r\n"));
    }

    public static (bool Found, string Text) GetLineContent(string[] lines, string start_line, string end_line) => GetLineContent(lines, x => x.StartsWith(start_line), x => x.StartsWith(end_line));

    public static (string Path, string TestName, string ErrorMessage) Compile(string src, string il)
    {
        var filename = Path.GetFileName(src);
        var txt = File.ReadAllText(src);
        var lines = txt.SplitLine();
        var testname = Path.GetFileNameWithoutExtension(src);

        try
        {
            FrontEnd.Compile(FrontEnd.Parse(new StringReader(txt)), il, ["System.Runtime"]);

            var valid = GetLineContent(lines, "###start", "###end");
            if (!valid.Found)
            {
                return (src, filename, "test code not found ###start - ###end");
            }
            var il_src = File.ReadAllText(il).Trim();

            if (valid.Text.Trim() != il_src) return (src, filename, "il make a difference");
        }
        catch (Exception ex)
        {
            var error = GetLineContent(lines, "###error", "###end");
            if (!error.Found || error.Text.Trim() != ex.Message)
            {
                return (src, filename, ex.Message);
            }
            File.WriteAllText(il, $@"
.assembly {filename} {{}}
.method public static void main()
{{
    .entrypoint
    ret
}}
");
        }
        return (src, filename, "");
    }

    [Test]
    public void CompileTest()
    {
        var compile_result = Directory.GetFiles(SourceDir, "*.rk")
            .MapParallelAllWithTimeout(x =>
            {
                var il = $"{Path.GetFileNameWithoutExtension(x)}.il";
                var result = Compile(x, il);
                File.Delete(il);
                return result;
            }, 1000 * 10, x => new(x, Path.GetFileName(x), "timeout"))
            .ToList();

        var failed = compile_result.Where(x => !x.Completed || x.Result.ErrorMessage != "").ToList();
        if (failed.Count > 0) Assert.Fail(failed.Select(x => $"{x.Result.TestName}: {x.Result.ErrorMessage}").Join("\n"));
    }
}
