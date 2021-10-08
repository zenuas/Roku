using Extensions;
using NUnit.Framework;
using Roku.Compiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Roku.Tests
{
    public class FrontEndTest
    {
        public static string SourceDir = "..\\..\\..\\rk";
        public static string ObjDir = "..\\..\\..\\rk\\obj";

        [SetUp]
        public void Setup()
        {
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new NoFailListener());
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            if (Directory.Exists(ObjDir))
            {
                Directory.GetFiles(ObjDir, "*.*").Each(File.Delete);
            }
            else
            {
                Directory.CreateDirectory(ObjDir);
            }
        }

        public static (bool Found, string Text) GetLineContent(string[] lines, Func<string, bool> start_line, Func<string, bool> end_line)
        {
            var start = lines.FindFirstIndex(x => start_line(x));
            if (start < 0) return (false, "not found start_line");
            var end = lines.Drop(start + 1).FindFirstIndex(x => end_line(x));
            if (end < 0) return (false, "not found start_line - end_line");
            return (true, lines[(start + 1)..(start + end + 1)].Join("\r\n"));
        }

        public static (bool Found, string Text) GetLineContent(string[] lines, string start_line, string end_line) => GetLineContent(lines, x => x.StartsWith(start_line), x => x.StartsWith(end_line));

        public static (string Path, string TestName, string ErrorMessage) Compile(string src)
        {
            var filename = Path.GetFileName(src);
            var txt = File.ReadAllText(src);
            var lines = txt.SplitLine();
            var testname = Path.GetFileNameWithoutExtension(src);
            var il = Path.Combine(ObjDir, testname + ".il");

            try
            {
                FrontEnd.Compile(new StringReader(txt), il, new string[] { "System.Runtime" });

                var valid = GetLineContent(lines, "###start", "###end");
                if (!valid.Found)
                {
                    return (src, filename, "test code not found ###start - ###end");
                }
                var il_src = File.ReadAllText(il).Trim();

                if (valid.Text.Trim() != il_src) return (src, filename, "il make a difference");

                File.Delete(il);
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
            var compile_result = Directory.GetFiles(SourceDir, "*.rk").MapParallelAllWithTimeout(Compile, 1000 * 10).ToList();

            var sjis = System.Text.Encoding.GetEncoding(932);
            var success = new List<string>();
            foreach (var result in compile_result)
            {
                var testname = Path.GetFileNameWithoutExtension(result.Result.Path);
                var in_ = Path.Combine(ObjDir, testname + ".testin");
                var out_ = Path.Combine(ObjDir, testname + ".testout");
                var err_ = Path.Combine(ObjDir, testname + ".testerr");
                var args_ = Path.Combine(ObjDir, testname + ".testargs");
                var name_ = Path.Combine(ObjDir, testname + ".testname");

                File.Delete(in_);
                File.Delete(out_);
                File.Delete(err_);
                File.Delete(args_);
                File.Delete(name_);

                var txt = File.ReadAllText(result.Result.Path);
                var lines = txt.SplitLine().Map(x => x.TrimStart()).ToArray();
                var name_p = GetLineContent(lines, x => x == "###", x => x == "###");
                var comment = testname + (name_p.Found ? $" {name_p.Text.SplitLine().Take(2).Join(" ").SubstringAsByte(0, 78 - testname.Length, sjis)}" : "");

                if (result.Completed && result.Result!.ErrorMessage == "")
                {
                    success.Add(comment);
                }
                else
                {
                    var in_p = lines.Where(x => x.StartsWith("#<=")).Map(x => x[3..] + "\r\n").Join();
                    var out_p = lines.Where(x => x.StartsWith("#=>")).Map(x => x[3..] + "\r\n").Join();
                    var err_p = lines.Where(x => x.StartsWith("#=2>")).Map(x => x[4..] + "\r\n").Join();
                    var args_p = lines.Where(x => x.StartsWith("##*")).Map(x => x[3..]).Join(" ");

                    File.WriteAllText(in_, in_p);
                    File.WriteAllText(out_, out_p);
                    File.WriteAllText(err_, err_p);
                    File.WriteAllText(args_, args_p);
                    File.WriteAllText(name_, comment);
                }
            }

            var skip = Path.Combine(ObjDir, ".success");
            File.Delete(skip);
            File.WriteAllText(skip, success.Count > 0 ? success.Join("\r\n") : "");

            var failed = compile_result.Where(x => x.Completed && x.Result!.ErrorMessage != "").ToList();
            if (failed.Count > 0) Assert.Fail(failed.Map(x => $"{x.Result.TestName}: {x.Result.ErrorMessage}").Join("\n"));
        }
    }
}
