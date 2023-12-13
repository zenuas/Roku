using Command;
using Extensions;
using Roku.Tests;
using System;
using System.IO;
using System.Linq;

namespace Roku.TestCompile;

public class Program
{
    [CommandOption('f')]
    [CommandOption("force")]
    public bool Force { get; init; } = false;

    [CommandOption('o')]
    [CommandOption("output")]
    public string Output { get; init; } = Path.Combine(".", "obj");

    public static void Main(string[] args)
    {
        var (opt, xargs) = CommandLine.Run<Program>(args);

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        if (Directory.Exists(opt.Output))
        {
            Directory.GetFiles(opt.Output, "*.*").Each(File.Delete);
        }
        else
        {
            _ = Directory.CreateDirectory(opt.Output);
        }

        var compile_result = Directory.GetFiles(".", "*.rk")
            .MapParallelAllWithTimeout(x => FrontEndTest.Compile(x, Path.Combine(opt.Output, $"{Path.GetFileNameWithoutExtension(x)}.il"), !opt.Force), 1000 * 10, x => new(x, Path.GetFileName(x), "timeout"))
            .ToList();

        var sjis = System.Text.Encoding.GetEncoding(932);
        foreach (var result in compile_result)
        {
            var testname = Path.GetFileNameWithoutExtension(result.Result.Path);
            var in_ = Path.Combine(opt.Output, testname + ".testin");
            var out_ = Path.Combine(opt.Output, testname + ".testout");
            var err_ = Path.Combine(opt.Output, testname + ".testerr");
            var args_ = Path.Combine(opt.Output, testname + ".testargs");
            var name_ = Path.Combine(opt.Output, testname + ".testname");

            File.Delete(in_);
            File.Delete(out_);
            File.Delete(err_);
            File.Delete(args_);
            File.Delete(name_);

            var txt = File.ReadAllText(result.Result.Path);
            var lines = txt.SplitLine().Select(x => x.TrimStart()).ToArray();
            var name_p = FrontEndTest.GetLineContent(lines, x => x == "###", x => x == "###");
            var comment = testname + (name_p.Found ? $" {name_p.Text.SplitLine().Take(2).Join(" ").SubstringAsByte(0, 78 - testname.Length, sjis)}" : "");

            Console.WriteLine(comment);
            if (opt.Force || !result.Completed || result.Result.ErrorMessage != "")
            {
                var in_p = lines.Where(x => x.StartsWith("#<=")).Select(x => x[3..] + "\r\n").Join();
                var out_p = lines.Where(x => x.StartsWith("#=>")).Select(x => x[3..] + "\r\n").Join();
                var err_p = lines.Where(x => x.StartsWith("#=2>")).Select(x => x[4..] + "\r\n").Join();
                var args_p = lines.Where(x => x.StartsWith("##*")).Select(x => x[3..]).Join(" ");

                File.WriteAllText(in_, in_p);
                File.WriteAllText(out_, out_p);
                File.WriteAllText(err_, err_p);
                File.WriteAllText(args_, args_p);
                File.WriteAllText(name_, comment);

                if (!result.Completed || result.Result.ErrorMessage != "")
                {
                    var prev_color = Console.ForegroundColor;
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(result.Result.ErrorMessage);
                    }
                    finally
                    {
                        Console.ForegroundColor = prev_color;
                    }
                }
            }
        }
    }
}
