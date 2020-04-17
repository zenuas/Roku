using Command;

namespace Roku
{
    public class Option
    {
        [ShortOption('o')]
        [LongOption("output")]
        string Output { get; set; } = "";

        [ShortOption('e')]
        [LongOption("entrypoint")]
        string EntryPoint { get; set; } = "";

        [ShortOption('l')]
        [LongOption("lib")]
        void LoadLibrary(string path)
        {

        }
    }
}
