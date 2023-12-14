using Command;
using System.Collections.Generic;

namespace Roku;

public class Option
{
    [CommandOption('o')]
    [CommandOption("output")]
    public string Output { get; set; } = "a.il";

    [CommandOption('e')]
    [CommandOption("entrypoint")]
    public string EntryPoint { get; set; } = "";

    public List<string> Libraries { get; set; } = [];

    [CommandOption('l')]
    [CommandOption("lib")]
    public void LoadLibrary(string path) => Libraries.Add(path);
}
