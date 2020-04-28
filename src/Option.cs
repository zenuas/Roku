using Command;

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

        [ShortOption('l')]
        [LongOption("lib")]
        public void LoadLibrary(string path)
        {

        }
    }
}
