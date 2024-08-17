using Mina.Extension;
using Roku.Compiler;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Roku.Test;

public class FrontEndTest
{
    public static string SourceDir = Path.Combine("..", "..", "..", "..", "test-rk");

    public FrontEndTest()
    {
        Trace.Listeners.Clear();
        _ = Trace.Listeners.Add(new NoFailListener());
    }

    public static (bool Found, string Text) GetLineContent(string[] lines, Func<string, bool> start_line, Func<string, bool> end_line)
    {
        var start = lines.FindFirstIndex(x => start_line(x));
        if (start < 0) return (false, "not found start_line");
        var end = lines.Skip(start + 1).FindFirstIndex(x => end_line(x));
        return end < 0
            ? (false, "not found start_line - end_line")
            : (true, lines[(start + 1)..(start + end + 1)].Join("\r\n"));
    }

    public static (bool Found, string Text) GetLineContent(string[] lines, string start_line, string end_line) => GetLineContent(lines, x => x.StartsWith(start_line), x => x.StartsWith(end_line));

    public static (string Path, string TestName, string ILName, string ErrorMessage, string ILText) Compile(string src, string il)
    {
        using var mem = new MemoryStream();
        using var writer = new StreamWriter(mem);
        return Compile(src, writer, mem, il);
    }

    public static (string Path, string TestName, string ILName, string ErrorMessage, string ILText) Compile(string src, TextWriter writer, MemoryStream mem, string il)
    {
        var filename = Path.GetFileName(src);
        var txt = File.ReadAllText(src);
        var lines = txt.SplitLine();

        try
        {
            FrontEnd.Compile(FrontEnd.Parse(new StringReader(txt)), writer, il, ["System.Runtime"]);

            var (found, text) = GetLineContent(lines, "###start", "###end");
            if (!found) return (src, filename, il, "test code not found ###start - ###end", "");

            _ = mem.Seek(0, SeekOrigin.Begin);
            var il_src = Encoding.UTF8.GetString(mem.EnumerableReadBytes().ToArray()).Trim();

            return (src, filename, il, text.Trim() == il_src ? "" : "il make a difference", il_src);
        }
        catch (Exception ex)
        {
            var (found, text) = GetLineContent(lines, "###error", "###end");
            return !found || text.Trim() != ex.Message
                ? (src, filename, il, ex.Message, "")
                : (src, filename, il, "", $@"
.assembly {filename} {{}}
.method public static void main()
{{
    .entrypoint
    ret
}}
");
        }
    }

    [Fact]
    public void CompileTest()
    {
        var compile_result = Directory.GetFiles(SourceDir, "*.rk")
            .Select(x => (RkName: x, ILName: $"{Path.GetFileNameWithoutExtension(x)}.il"))
            .MapParallelAllWithTimeout(x => Compile(x.RkName, x.ILName), 1000 * 10, x => new(x.RkName, Path.GetFileName(x.RkName), x.ILName, "timeout", ""))
            .ToList();

        var failed = compile_result.Where(x => !x.Completed || x.Result.ErrorMessage != "").ToList();
        if (failed.Count > 0) Assert.Fail(failed.Select(x => $"{x.Result.TestName}: {x.Result.ErrorMessage}").Join("\n"));
    }

    [Fact]
    public void Compile2TimeTest()
    {
        using var mem = new MemoryStream();
        using var writer = new StreamWriter(mem);
        FrontEnd.Compile(FrontEnd.Parse(new StringReader("print(1)")), writer, "a.il", ["System.Runtime"]);
        FrontEnd.Compile(FrontEnd.Parse(new StringReader("print(2)")), writer, "b.il", ["System.Runtime"]);

        _ = mem.Seek(0, SeekOrigin.Begin);
        var il_src = Encoding.UTF8.GetString(mem.EnumerableReadBytes().ToArray()).Trim();
        Assert.Equal(il_src,
@".assembly extern System.Console { .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A) }
.assembly extern System.Runtime { .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A) }
.assembly a {}

.class interface public abstract '#RTTI'
{
    .method public hidebysig newslot abstract virtual instance int32 '#GetTypeNo'() {}
    .method public hidebysig newslot abstract virtual instance class [System.Runtime]System.Type[] '#GetTypeGenerics'() {}
}

.method public static void main()
{
    .entrypoint
    .maxstack 1
    
    ldc.i4.1
    call void class [System.Console]System.Console::WriteLine(int32)
    ret
}
.assembly extern System.Console { .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A) }
.assembly extern System.Runtime { .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A) }
.assembly b {}

.class interface public abstract '#RTTI'
{
    .method public hidebysig newslot abstract virtual instance int32 '#GetTypeNo'() {}
    .method public hidebysig newslot abstract virtual instance class [System.Runtime]System.Type[] '#GetTypeGenerics'() {}
}

.method public static void main()
{
    .entrypoint
    .maxstack 1
    
    ldc.i4.2
    call void class [System.Console]System.Console::WriteLine(int32)
    ret
}");
    }
}
