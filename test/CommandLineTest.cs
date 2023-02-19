using Command;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Tests;

public class Option
{
    [ShortOption('o')]
    [LongOption("output")]
    public string Output { get; set; } = "";

    [ShortOption('e')]
    [LongOption("entrypoint")]
    public string EntryPoint { get; set; } = "";

    public List<string> Lib { get; private set; } = new List<string>();

    [ShortOption('l')]
    [LongOption("lib")]
    public void LoadLibrary(string path) => Lib.Add(path);

    [ShortOption('t')]
    [LongOption("test")]
    public void Test() => Lib.Add("xxx");
}

public class CommandLineTest
{
    [Test]
    public void Test()
    {
        var receiver = new Option();
        var args = CommandLine.Run(receiver, "a", "-otest1", "--entrypoint", "test2", "-t", "-l", "test3", "b", "--lib", "test4", "c");

        Assert.AreEqual(receiver.Output, "test1");
        Assert.AreEqual(receiver.EntryPoint, "test2");
        Assert.AreEqual(receiver.Lib, new string[] { "xxx", "test3", "test4" });
        Assert.AreEqual(args, new string[] { "a", "b", "c" });
    }

    [Test]
    public void OutputStdout()
    {
        var receiver = new Option();
        _ = CommandLine.Run(receiver, "-o", "-");

        Assert.AreEqual(receiver.Output, "-");
    }
}
