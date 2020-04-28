using Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Roku.Tests
{
    public class FrontEndTest
    {
        public string SourceDir = "..\\..\\..\\rk";
        public string ObjDir = Path.Combine("..\\..\\..\\rk", "obj");

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (Directory.Exists(ObjDir)) Directory.Delete(ObjDir, true);
            Directory.CreateDirectory(ObjDir);
        }

        [Test]
        public void CompileTest()
        {
            var failed = new List<(string Path, string Message)>();
            foreach (var src in Directory.GetFiles(SourceDir, "*.rk"))
            {
                var filename = Path.GetFileName(src);
                try
                {
                    var il = Path.Combine(ObjDir, Path.GetFileNameWithoutExtension(src) + ".il");
                    var txt = File.ReadAllText(src);
                    FrontEnd.Compile(new StringReader(txt), il);

                    var lines = txt.SplitLine();
                    var start = lines.FindFirstIndex(x => x.StartsWith("###start"));
                    var end = lines.FindFirstIndex(x => x.StartsWith("###end"));
                    var valid = lines[(start + 1)..end].Join("\r\n").Trim();
                    var il_src = File.ReadAllText(il).Trim();

                    if (valid != il_src) failed.Add((filename, "il make a difference"));
                }
                catch (Exception ex)
                {
                    failed.Add((filename, ex.Message));
                }
            }
            if (failed.Count > 0) Assert.Fail(failed.Map(x => $"{x.Path}: {x.Message}").Join("\n"));
        }

        [Test]
        public void MakeTestCaseTest()
        {
            foreach (var src in Directory.GetFiles(SourceDir, "*.rk"))
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
            }
            Assert.Pass();
        }
    }
}
