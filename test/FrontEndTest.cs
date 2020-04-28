using Extensions;
using NUnit.Framework;
using System.IO;

namespace Roku.Tests
{
    class FrontEndTest
    {
        [Test]
        public void CompileTest()
        {
            var src_dir = "..\\..\\..\\rk";
            var obj_dir = Path.Combine(src_dir, "obj");
            Directory.CreateDirectory(obj_dir);

            foreach (var src in Directory.GetFiles(src_dir, "*.rk"))
            {
                var il = Path.Combine(obj_dir, Path.GetFileNameWithoutExtension(src) + ".il");
                var txt = File.ReadAllText(src);
                FrontEnd.Compile(new StringReader(txt), il);

                var lines = txt.Split("\r\n");
                var start = lines.FindFirstIndex(x => x.StartsWith("###start"));
                var end = lines.FindFirstIndex(x => x.StartsWith("###end"));
                var valid = lines[(start + 1)..end].Join("\r\n").Trim();
                var il_src = File.ReadAllText(il).Trim();

                Assert.AreEqual(valid, il_src, Path.GetFileName(src));
            }
        }
    }
}
