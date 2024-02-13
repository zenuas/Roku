using Mina.Command;
using Mina.Extension;
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
        if (opt.Force) _ = Directory.CreateDirectory(opt.Output);

        var compile_result = Directory.GetFiles(".", "*.rk")
            .Select(x => (RkName: x, ILName: Path.Combine(opt.Output, $"{Path.GetFileNameWithoutExtension(x)}.il")))
            .MapParallelAllWithTimeout(x => FrontEndTest.Compile(x.RkName, x.ILName), 1000 * 10, x => new(x.RkName, Path.GetFileName(x.RkName), x.ILName, "timeout", ""))
            .ToList();

        var sjis = System.Text.Encoding.GetEncoding(932);
        foreach (var result in compile_result)
        {
            var testname = Path.GetFileNameWithoutExtension(result.Result.Path);
            var txt = File.ReadAllText(result.Result.Path);
            var lines = txt.SplitLine().Select(x => x.TrimStart()).ToArray();
            var name_p = FrontEndTest.GetLineContent(lines, x => x == "###", x => x == "###");
            var comment = testname + (name_p.Found ? $" {name_p.Text.SplitLine().Take(2).Join(" ").SubstringAsByte(0, 78 - testname.Length, sjis)}" : "");

            Console.WriteLine(comment);
            if (opt.Force || !result.Completed || result.Result.ErrorMessage != "")
            {
                _ = Directory.CreateDirectory(opt.Output);
                if (!opt.Force) File.WriteAllText(result.Result.ILName, result.Result.ILText);

                var in_p = lines.Where(x => x.StartsWith("#<=")).Select(x => x[3..] + "\r\n").Join();
                var out_p = lines.Where(x => x.StartsWith("#=>")).Select(x => x[3..] + "\r\n").Join();
                var err_p = lines.Where(x => x.StartsWith("#=2>")).Select(x => x[4..] + "\r\n").Join();
                var args_p = lines.Where(x => x.StartsWith("##*")).Select(x => x[3..]).Join(" ");

                File.WriteAllText(Path.Combine(opt.Output, testname + ".testin"), in_p);
                File.WriteAllText(Path.Combine(opt.Output, testname + ".testout"), out_p);
                File.WriteAllText(Path.Combine(opt.Output, testname + ".testerr"), err_p);
                File.WriteAllText(Path.Combine(opt.Output, testname + ".testargs"), args_p);

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
