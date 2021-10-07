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
        public string SourceDir = "..\\..\\..\\rk";
        public string ObjDir = "..\\..\\..\\rk\\obj";

        [SetUp]
        public void Setup()
        {
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new NoFailListener());
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (Directory.Exists(ObjDir))
            {
                Directory.GetFiles(ObjDir, "*.*").Each(File.Delete);
            }
            else
            {
                Directory.CreateDirectory(ObjDir);
            }
        }

        public (bool Found, string Text) GetLineContent(string[] lines, Func<string, bool> start_line, Func<string, bool> end_line)
        {
            var start = lines.FindFirstIndex(x => start_line(x));
            if (start < 0) return (false, "not found start_line");
            var end = lines.Drop(start + 1).FindFirstIndex(x => end_line(x));
            if (end < 0) return (false, "not found start_line - end_line");
            return (true, lines[(start + 1)..(start + end + 1)].Join("\r\n"));
        }

        public (bool Found, string Text) GetLineContent(string[] lines, string start_line, string end_line) => GetLineContent(lines, x => x.StartsWith(start_line), x => x.StartsWith(end_line));

        [Test, Order(1)]
        public void CompileTest()
        {
            var failed = new List<(string Path, string Message)>(
                Directory.GetFiles(SourceDir, "*.rk").MapParallelAllWithTimeout(src =>
                {
                    var filename = Path.GetFileName(src);
                    var txt = File.ReadAllText(src);
                    var lines = txt.SplitLine();
                    var testname = Path.GetFileNameWithoutExtension(src);
                    var il = Path.Combine(ObjDir, testname + ".il");
                    var skip = Path.Combine(ObjDir, testname + ".testskip");

                    File.Delete(skip);
                    try
                    {
                        FrontEnd.Compile(new StringReader(txt), il, new string[] { "System.Runtime" });

                        var valid = GetLineContent(lines, "###start", "###end");
                        if (!valid.Found)
                        {
                            return Tuple.Create(filename, "test code not found ###start - ###end");
                        }
                        var il_src = File.ReadAllText(il).Trim();

                        if (valid.Text.Trim() != il_src) return Tuple.Create(filename, "il make a difference");

                        File.WriteAllText(skip, "true");
                    }
                    catch (Exception ex)
                    {
                        var error = GetLineContent(lines, "###error", "###end");
                        if (!error.Found || error.Text.Trim() != ex.Message)
                        {
                            return Tuple.Create(filename, ex.Message);
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
                    return Tuple.Create(filename, "");
                }, 1000 * 10)
                .Where(x => x.Completed && x.Result!.Item2 != "")
                .Map(x => (x.Result!.Item1, x.Result!.Item2)));

            if (failed.Count > 0) Assert.Fail(failed.Map(x => $"{x.Path}: {x.Message}").Join("\n"));
        }

        [Test, Order(2)]
        public void MakeTestCaseTest()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var sjis = System.Text.Encoding.GetEncoding(932);

            Directory.GetFiles(SourceDir, "*.rk").AsParallel().Each(src =>
            {
                var testname = Path.GetFileNameWithoutExtension(src);
                var in_ = Path.Combine(ObjDir, testname + ".testin");
                var out_ = Path.Combine(ObjDir, testname + ".testout");
                var err_ = Path.Combine(ObjDir, testname + ".testerr");
                var args_ = Path.Combine(ObjDir, testname + ".testargs");
                var name_ = Path.Combine(ObjDir, testname + ".testname");
                var skip = Path.Combine(ObjDir, testname + ".testskip");

                File.Delete(in_);
                File.Delete(out_);
                File.Delete(err_);
                File.Delete(args_);
                File.Delete(name_);

                var txt = File.ReadAllText(src);
                var lines = txt.SplitLine().Map(x => x.TrimStart()).ToArray();

                if (!File.Exists(skip))
                {
                    var in_p = lines.Where(x => x.StartsWith("#<=")).Map(x => x[3..] + "\r\n").Join();
                    var out_p = lines.Where(x => x.StartsWith("#=>")).Map(x => x[3..] + "\r\n").Join();
                    var err_p = lines.Where(x => x.StartsWith("#=2>")).Map(x => x[4..] + "\r\n").Join();
                    var args_p = lines.Where(x => x.StartsWith("##*")).Map(x => x[3..]).Join(" ");

                    File.WriteAllText(in_, in_p);
                    File.WriteAllText(out_, out_p);
                    File.WriteAllText(err_, err_p);
                    File.WriteAllText(args_, args_p);
                }

                var name_p = GetLineContent(lines, x => x == "###", x => x == "###");
                File.WriteAllText(name_, Path.GetFileNameWithoutExtension(src) + (name_p.Found ? $" {SubstringAsByte(name_p.Text.SplitLine().Take(2).Join(" "), 78 - testname.Length, sjis)}" : ""));
            });
            Assert.Pass();
        }

        public static string SubstringAsByte(string self, int length, System.Text.Encoding enc) =>
            self.Substring(0,
                self.Map(x => enc.GetByteCount(new char[] { x })).
                Accumulator((acc, x) => acc + x).
                Take(x => x <= length).
                Count());
    }
}
