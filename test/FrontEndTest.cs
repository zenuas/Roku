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

        public (bool Found, string Text) GetLineContent(string[] lines, string start_line, string end_line)
        {
            var start = lines.FindFirstIndex(x => x.StartsWith(start_line));
            if (start < 0) return (false, $"not found {start_line}");
            var end = lines.Drop(start + 1).FindFirstIndex(x => x.StartsWith("###end"));
            if (end < 0) return (false, $"not found {start_line} - {end_line}");
            return (true, lines[(start + 1)..(start + end + 1)].Join("\r\n"));
        }

        [Test]
        public void CompileTest()
        {
            var failed = new List<(string Path, string Message)>(
                Directory.GetFiles(SourceDir, "*.rk").MapParallelAll(src =>
                {
                    var filename = Path.GetFileName(src);
                    var txt = File.ReadAllText(src);
                    var lines = txt.SplitLine();
                    var il = Path.Combine(ObjDir, Path.GetFileNameWithoutExtension(src) + ".il");
                    try
                    {
                        FrontEnd.Compile(new StringReader(txt), il, new string[] { "System.Runtime" });

                        var valid = GetLineContent(lines, "###start", "###end");
                        if (!valid.Found)
                        {
                            return (filename, "test code not found ###start - ###end");
                        }
                        var il_src = File.ReadAllText(il).Trim();

                        if (valid.Text.Trim() != il_src) return (filename, "il make a difference");
                    }
                    catch (Exception ex)
                    {
                        var error = GetLineContent(lines, "###error", "###end");
                        if (!error.Found || error.Text.Trim() != ex.Message)
                        {
                            return (filename, ex.Message);
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
                    return (filename, "");
                }).Where(x => x.Item2 != ""));

            if (failed.Count > 0) Assert.Fail(failed.Map(x => $"{x.Path}: {x.Message}").Join("\n"));
        }

        [Test]
        public void MakeTestCaseTest()
        {
            Directory.GetFiles(SourceDir, "*.rk").AsParallel().Each(src =>
            {
                var in_ = Path.Combine(ObjDir, Path.GetFileNameWithoutExtension(src) + ".testin");
                var out_ = Path.Combine(ObjDir, Path.GetFileNameWithoutExtension(src) + ".testout");
                var err_ = Path.Combine(ObjDir, Path.GetFileNameWithoutExtension(src) + ".testerr");
                var args_ = Path.Combine(ObjDir, Path.GetFileNameWithoutExtension(src) + ".testargs");

                var txt = File.ReadAllText(src);
                var lines = txt.SplitLine().Map(x => x.TrimStart()).ToArray();

                var in_p = lines.Where(x => x.StartsWith("#<=")).Map(x => x[3..] + "\r\n").Join();
                var out_p = lines.Where(x => x.StartsWith("#=>")).Map(x => x[3..] + "\r\n").Join();
                var err_p = lines.Where(x => x.StartsWith("#=2>")).Map(x => x[4..] + "\r\n").Join();
                var args_p = lines.Where(x => x.StartsWith("##*")).Map(x => x[3..]).Join(" ");

                File.Delete(in_);
                File.Delete(out_);
                File.Delete(err_);
                File.Delete(args_);

                File.WriteAllText(in_, in_p);
                File.WriteAllText(out_, out_p);
                File.WriteAllText(err_, err_p);
                File.WriteAllText(args_, args_p);
            });
            Assert.Pass();
        }
    }
}
