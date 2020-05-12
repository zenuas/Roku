using Command;
using System.Collections.Generic;

namespace Roku
{
    public class Option
    {
        [ShortOption('o')]
        [LongOption("output")]
        public string Output { get; set; } = "a.il";

        [ShortOption('e')]
        [LongOption("entrypoint")]
        public string EntryPoint { get; set; } = "";

        public List<string> Libraries { get; set; } = new List<string>();

        [ShortOption('l')]
        [LongOption("lib")]
        public void LoadLibrary(string path) => Libraries.Add(path);
    }
}
